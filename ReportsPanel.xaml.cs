using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using UniversitySystem.Data;
using UniversitySystem.Models;
using UniversitySystem.Services;

namespace UniversitySystem;

public partial class ReportsPanel : UserControl
{
    private List<Group> _groups = new();
    private List<Discipline> _disciplines = new();

    public ReportsPanel()
    {
        InitializeComponent();
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        await LoadGroupsAsync();
        await LoadDisciplinesAsync();
    }

    private async Task LoadGroupsAsync()
    {
        await using var db = new AppDbContext();
        _groups = await db.Groups.Include(g => g.Department).AsNoTracking().ToListAsync();
        
        CmbGroupForRating.ItemsSource = _groups;
        CmbGroupForRating.DisplayMemberPath = "Name";
        CmbGroupForRating.SelectedValuePath = "Id";

        CmbGroupForStats.ItemsSource = _groups;
        CmbGroupForStats.DisplayMemberPath = "Name";
        CmbGroupForStats.SelectedValuePath = "Id";

        CmbGroupForDebtors.ItemsSource = _groups;
        CmbGroupForDebtors.DisplayMemberPath = "Name";
        CmbGroupForDebtors.SelectedValuePath = "Id";
    }

    private async Task LoadDisciplinesAsync()
    {
        await using var db = new AppDbContext();
        _disciplines = await db.Disciplines.AsNoTracking().ToListAsync();
        
        CmbDisciplineForRating.ItemsSource = _disciplines;
        CmbDisciplineForRating.DisplayMemberPath = "Name";
        CmbDisciplineForRating.SelectedValuePath = "Id";
    }

    private async void BtnGenerateRating_Click(object sender, RoutedEventArgs e)
    {
        if (CmbGroupForRating.SelectedValue == null || CmbDisciplineForRating.SelectedValue == null)
        {
            MessageBox.Show("Выберите группу и дисциплину", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            await using var db = new AppDbContext();
            var groupId = (int)CmbGroupForRating.SelectedValue;
            var disciplineId = (int)CmbDisciplineForRating.SelectedValue;

            var rating = await db.Grades
                .Include(g => g.Student)
                .ThenInclude(s => s.Group)
                .Where(g => g.DisciplineId == disciplineId && g.Student.GroupId == groupId)
                .GroupBy(g => g.StudentId)
                .Select(g => new
                {
                    StudentName = g.FirstOrDefault()!.Student.FullName,
                    GroupName = g.FirstOrDefault()!.Student.Group.Name,
                    AverageGrade = g.Average(x => x.Value)
                })
                .OrderByDescending(x => x.AverageGrade)
                .ToListAsync();

            if (rating.Count == 0)
            {
                MessageBox.Show("Нет данных для формирования рейтинга", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                DgRating.ItemsSource = null;
                return;
            }

            DgRating.ItemsSource = rating.Select((r, index) => new
            {
                Rank = index + 1,
                r.StudentName,
                r.GroupName,
                r.AverageGrade
            }).ToList();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void BtnExportRating_Click(object sender, RoutedEventArgs e)
    {
        if (DgRating.ItemsSource == null || DgRating.Items.Count == 0)
        {
            MessageBox.Show("Сначала сформируйте рейтинг", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                FileName = $"Рейтинг_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            };

            if (saveDialog.ShowDialog() == true)
            {
                await using var db = new AppDbContext();
                var groupId = (int)CmbGroupForRating.SelectedValue;
                var disciplineId = (int)CmbDisciplineForRating.SelectedValue;

                var ratingData = await db.Grades
                    .Include(g => g.Student)
                    .ThenInclude(s => s.Group)
                    .Where(g => g.DisciplineId == disciplineId && g.Student.GroupId == groupId)
                    .GroupBy(g => g.StudentId)
                    .Select(g => new
                    {
                        StudentName = g.FirstOrDefault()!.Student.FullName,
                        GroupName = g.FirstOrDefault()!.Student.Group.Name,
                        AverageGrade = g.Average(x => x.Value)
                    })
                    .OrderByDescending(x => x.AverageGrade)
                    .ToListAsync();

                // Create anonymous list with Rank
                var rankedData = ratingData.Select((r, index) => new { Rank = index + 1, r.StudentName, r.GroupName, r.AverageGrade }).ToList();
                
                await ExportService.ExportGradesToExcelAsync(
                    rankedData.Select(r => new Grade { Student = new User { FullName = r.StudentName }, Value = (decimal)r.AverageGrade }).ToList(),
                    saveDialog.FileName);

                MessageBox.Show($"Рейтинг экспортирован в {saveDialog.FileName}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void BtnShowStats_Click(object sender, RoutedEventArgs e)
    {
        if (CmbGroupForStats.SelectedValue == null)
        {
            MessageBox.Show("Выберите группу", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            await using var db = new AppDbContext();
            var groupId = (int)CmbGroupForStats.SelectedValue;

            var students = await db.Users.Where(u => u.GroupId == groupId && u.Role == "Student").ToListAsync();
            var grades = await db.Grades.Where(g => students.Select(s => s.Id).Contains(g.StudentId)).ToListAsync();

            if (grades.Count > 0)
            {
                var average = grades.Average(g => g.Value);
                var excellent = grades.Count(g => g.Value >= 5);
                var good = grades.Count(g => g.Value >= 4 && g.Value < 5);
                var satisfactory = grades.Count(g => g.Value >= 3 && g.Value < 4);
                var fail = grades.Count(g => g.Value < 3);

                TxtGroupAverage.Text = average.ToString("F2");
                TxtStudentCount.Text = students.Count.ToString();
                TxtExcellent.Text = excellent.ToString();
                TxtGood.Text = good.ToString();
                TxtSatisfactory.Text = satisfactory.ToString();
                TxtFail.Text = fail.ToString();
            }
            else
            {
                TxtGroupAverage.Text = "0.00";
                TxtStudentCount.Text = students.Count.ToString();
                TxtExcellent.Text = "0";
                TxtGood.Text = "0";
                TxtSatisfactory.Text = "0";
                TxtFail.Text = "0";
                MessageBox.Show("Нет оценок для выбранной группы", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void BtnExportStats_Click(object sender, RoutedEventArgs e)
    {
        if (CmbGroupForStats.SelectedValue == null)
        {
            MessageBox.Show("Сначала выберите группу и сформируйте статистику", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                FileName = $"Статистика_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            };

            if (saveDialog.ShowDialog() == true)
            {
                await using var db = new AppDbContext();
                var groupId = (int)CmbGroupForStats.SelectedValue;
                var group = await db.Groups.FirstOrDefaultAsync(g => g.Id == groupId);
                var students = await db.Users.Where(u => u.GroupId == groupId && u.Role == "Student").ToListAsync();
                var grades = await db.Grades.Where(g => students.Select(s => s.Id).Contains(g.StudentId)).ToListAsync();

                // Build statistics report
                var avg = grades.Count > 0 ? grades.Average(g => g.Value) : 0;
                var excellent = grades.Count(g => g.Value >= 5);
                var good = grades.Count(g => g.Value >= 4 && g.Value < 5);
                var satisfactory = grades.Count(g => g.Value >= 3 && g.Value < 4);
                var fail = grades.Count(g => g.Value < 3);

                using var workbook = new ClosedXML.Excel.XLWorkbook();
                var ws = workbook.Worksheets.Add("Статистика группы");
                
                ws.Cell(1, 1).Value = "Статистика группы";
                ws.Cell(1, 1).Style.Font.Bold = true;
                ws.Cell(1, 1).Style.Font.FontSize = 16;
                ws.Range(1, 1, 1, 2).Merge();

                ws.Cell(3, 1).Value = "Группа:";
                ws.Cell(3, 2).Value = group?.Name ?? "";
                ws.Cell(4, 1).Value = "Количество студентов:";
                ws.Cell(4, 2).Value = students.Count;
                ws.Cell(5, 1).Value = "Средний балл:";
                ws.Cell(5, 2).Value = avg.ToString("F2");
                ws.Cell(7, 1).Value = "Отлично (5):";
                ws.Cell(7, 2).Value = excellent;
                ws.Cell(8, 1).Value = "Хорошо (4):";
                ws.Cell(8, 2).Value = good;
                ws.Cell(9, 1).Value = "Удовлетворительно (3):";
                ws.Cell(9, 2).Value = satisfactory;
                ws.Cell(10, 1).Value = "Неудовлетворительно (2):";
                ws.Cell(10, 2).Value = fail;

                workbook.SaveAs(saveDialog.FileName);
                MessageBox.Show($"Статистика экспортирована в {saveDialog.FileName}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void BtnFindDebtors_Click(object sender, RoutedEventArgs e)
    {
        if (CmbGroupForDebtors.SelectedValue == null)
        {
            MessageBox.Show("Выберите группу", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            await using var db = new AppDbContext();
            var groupId = (int)CmbGroupForDebtors.SelectedValue;

            var students = await db.Users.Where(u => u.GroupId == groupId && u.Role == "Student").ToListAsync();
            var debtors = await db.Grades
                .Include(g => g.Student)
                .ThenInclude(s => s.Group)
                .Include(g => g.Discipline)
                .Where(g => students.Select(s => s.Id).Contains(g.StudentId) && g.Value < 3)
                .GroupBy(g => new { g.StudentId, g.DisciplineId })
                .Select(g => new
                {
                    StudentName = g.FirstOrDefault()!.Student.FullName,
                    GroupName = g.FirstOrDefault()!.Student.Group.Name,
                    DisciplineName = g.FirstOrDefault()!.Discipline.Name,
                    DebtCount = g.Count()
                })
                .ToListAsync();

            if (debtors.Count == 0)
            {
                MessageBox.Show("Должники не найдены", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            DgDebtors.ItemsSource = debtors;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}