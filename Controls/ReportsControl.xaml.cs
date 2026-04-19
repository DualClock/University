using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using UniversitySystem.Data;
using UniversitySystem.Models;
using UniversitySystem.Services;

namespace UniversitySystem.Controls
{
    public partial class ReportsControl : UserControl
    {
        private List<Group> _groups = new();
        private List<Faculty> _faculties = new();
        
        public ReportsControl()
        {
            InitializeComponent();
            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            await LoadGroupsAsync();
            await LoadFacultiesAsync();
        }

        private async Task LoadGroupsAsync()
        {
            using var db = new AppDbContext();
            _groups = await db.Groups
                .Include(g => g.Department)
                .ThenInclude(d => d!.Faculty)
                .AsNoTracking()
                .ToListAsync();
            GroupFilter.ItemsSource = _groups;
        }

        private async Task LoadFacultiesAsync()
        {
            using var db = new AppDbContext();
            _faculties = await db.Faculties.AsNoTracking().ToListAsync();
            FacultyFilter.ItemsSource = _faculties;
        }

        private async void GroupFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await GenerateGroupReport();
        }

        private async void FacultyFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await GenerateFacultyReport();
        }

        private async void GenerateReport_Click(object sender, RoutedEventArgs e)
        {
            if (TabControlReports.SelectedIndex == 0)
                await GenerateGroupReport();
            else
                await GenerateFacultyReport();
        }

        private async Task GenerateGroupReport()
        {
            if (GroupFilter.SelectedItem == null) return;

            try
            {
                using var db = new AppDbContext();
                int gid = (GroupFilter.SelectedItem as Group)?.Id ?? 0;

                var students = await db.Users
                    .Where(u => u.GroupId == gid && u.Role == "Student")
                    .Include(u => u.Group)
                    .AsNoTracking()
                    .ToListAsync();

                var grades = await db.Grades
                    .Where(g => students.Select(s => s.Id).Contains(g.StudentId))
                    .AsNoTracking()
                    .ToListAsync();

                if (students.Count == 0)
                {
                    MessageBox.Show("В выбранной группе нет студентов", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    ReportDataGrid.ItemsSource = null;
                    return;
                }

                var report = students.Select(s =>
                {
                    var sg = grades.Where(g => g.StudentId == s.Id).ToList();
                    var avg = sg.Count > 0 ? sg.Average(g => (double)g.Value) : 0;
                    var status = avg >= 4.5 ? "Отличник" : avg >= 3.5 ? "Хорошист" : avg >= 2 ? "Троечник" : "Должник";
                    return new
                    {
                        StudentName = s.FullName,
                        GroupName = s.Group?.Name ?? "",
                        AvgGrade = avg,
                        Status = status
                    };
                }).ToList();

                ReportDataGrid.ItemsSource = report;

                ExcellentCount.Text = report.Count(r => r.Status == "Отличник").ToString();
                GoodCount.Text = report.Count(r => r.Status == "Хорошист").ToString();
                FailCount.Text = report.Count(r => r.Status == "Должник").ToString();

                var overallAvg = report.Count > 0 ? report.Average(r => r.AvgGrade) : 0;
                TxtAverageGrade.Text = overallAvg.ToString("F2");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка формирования отчёта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task GenerateFacultyReport()
        {
            if (FacultyFilter.SelectedItem == null) return;

            try
            {
                using var db = new AppDbContext();
                int facultyId = (FacultyFilter.SelectedItem as Faculty)?.Id ?? 0;

                var departments = await db.Departments
                    .Where(d => d.FacultyId == facultyId)
                    .AsNoTracking()
                    .ToListAsync();

                var groups = await db.Groups
                    .Where(g => departments.Select(d => d.Id).Contains(g.DepartmentId))
                    .AsNoTracking()
                    .ToListAsync();

                var students = await db.Users
                    .Where(u => u.Role == "Student" && u.GroupId != null && groups.Select(g => g.Id).Contains(u.GroupId.Value))
                    .Include(u => u.Group)
                    .ThenInclude(g => g!.Department)
                    .AsNoTracking()
                    .ToListAsync();

                var grades = await db.Grades
                    .Where(g => students.Select(s => s.Id).Contains(g.StudentId))
                    .AsNoTracking()
                    .ToListAsync();

                if (students.Count == 0)
                {
                    MessageBox.Show("В выбранном факультете нет студентов", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    FacultyReportDataGrid.ItemsSource = null;
                    return;
                }

                var facultyName = (FacultyFilter.SelectedItem as Faculty)?.Name ?? "";

                var report = departments.Select(d =>
                {
                    var deptGroups = groups.Where(g => g.DepartmentId == d.Id).ToList();
                    var deptStudents = students.Where(s => s.Group != null && deptGroups.Select(g => g.Id).Contains(s.Group.Id)).ToList();
                    var deptGrades = grades.Where(g => deptStudents.Select(s => s.Id).Contains(g.StudentId)).ToList();
                    var avg = deptGrades.Count > 0 ? deptGrades.Average(g => (double)g.Value) : 0;

                    return new
                    {
                        DepartmentName = d.Name,
                        GroupCount = deptGroups.Count,
                        StudentCount = deptStudents.Count,
                        AvgGrade = avg
                    };
                }).Where(r => r.StudentCount > 0).ToList();

                FacultyReportDataGrid.ItemsSource = report;

                TxtFacultyTotalGroups.Text = groups.Count.ToString();
                TxtFacultyTotalStudents.Text = students.Count.ToString();
                var overallAvg = report.Count > 0 ? report.Average(r => r.AvgGrade) : 0;
                TxtFacultyAverage.Text = overallAvg.ToString("F2");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка формирования отчёта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnExportGroupReport_Click(object sender, RoutedEventArgs e)
        {
            if (ReportDataGrid.ItemsSource == null || ReportDataGrid.Items.Count == 0)
            {
                MessageBox.Show("Сначала сформируйте отчёт по группе", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = $"Отчёт_группа_{DateTime.Now:yyyyMMdd_HHmmss}",
                    DefaultExt = ".xlsx",
                    Filter = "Excel Files|*.xlsx",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var groupName = (GroupFilter.SelectedItem as Group)?.Name ?? "";
                    var reportData = ReportDataGrid.ItemsSource as IEnumerable<dynamic>;
                    
                    using var workbook = new ClosedXML.Excel.XLWorkbook();
                    var ws = workbook.Worksheets.Add("Отчёт по группе");

                    ws.Cell(1, 1).Value = $"Отчёт по успеваемости группы: {groupName}";
                    ws.Cell(1, 1).Style.Font.Bold = true;
                    ws.Cell(1, 1).Style.Font.FontSize = 14;

                    ws.Cell(3, 1).Value = "Студент";
                    ws.Cell(3, 2).Value = "Группа";
                    ws.Cell(3, 3).Value = "Средний балл";
                    ws.Cell(3, 4).Value = "Статус";

                    int row = 4;
                    foreach (var item in reportData)
                    {
                        ws.Cell(row, 1).Value = item.StudentName?.ToString() ?? "";
                        ws.Cell(row, 2).Value = item.GroupName?.ToString() ?? "";
                        ws.Cell(row, 3).Value = item.AvgGrade;
                        ws.Cell(row, 4).Value = item.Status?.ToString() ?? "";
                        row++;
                    }

                    workbook.SaveAs(saveDialog.FileName);
                    MessageBox.Show($"Отчёт экспортирован в {saveDialog.FileName}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnExportFacultyReport_Click(object sender, RoutedEventArgs e)
        {
            if (FacultyReportDataGrid.ItemsSource == null || FacultyReportDataGrid.Items.Count == 0)
            {
                MessageBox.Show("Сначала сформируйте отчёт по факультету", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = $"Отчёт_факультет_{DateTime.Now:yyyyMMdd_HHmmss}",
                    DefaultExt = ".xlsx",
                    Filter = "Excel Files|*.xlsx",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var facultyName = (FacultyFilter.SelectedItem as Faculty)?.Name ?? "";
                    var reportData = FacultyReportDataGrid.ItemsSource as IEnumerable<dynamic>;
                    
                    using var workbook = new ClosedXML.Excel.XLWorkbook();
                    var ws = workbook.Worksheets.Add("Отчёт по факультету");

                    ws.Cell(1, 1).Value = $"Отчёт по успеваемости факультета: {facultyName}";
                    ws.Cell(1, 1).Style.Font.Bold = true;
                    ws.Cell(1, 1).Style.Font.FontSize = 14;

                    ws.Cell(3, 1).Value = "Кафедра";
                    ws.Cell(3, 2).Value = "Кол-во групп";
                    ws.Cell(3, 3).Value = "Кол-во студентов";
                    ws.Cell(3, 4).Value = "Средний балл";

                    int row = 4;
                    foreach (var item in reportData)
                    {
                        ws.Cell(row, 1).Value = item.DepartmentName?.ToString() ?? "";
                        ws.Cell(row, 2).Value = item.GroupCount;
                        ws.Cell(row, 3).Value = item.StudentCount;
                        ws.Cell(row, 4).Value = item.AvgGrade;
                        row++;
                    }

                    workbook.SaveAs(saveDialog.FileName);
                    MessageBox.Show($"Отчёт экспортирован в {saveDialog.FileName}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}