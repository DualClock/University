using System;
using System.IO;
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
    public partial class ExportControl : UserControl
    {
        public ExportControl()
        {
            InitializeComponent();
        }

        private async void ExportGrades_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = $"Оценки_{DateTime.Now:yyyyMMdd_HHmmss}",
                    DefaultExt = ".xlsx",
                    Filter = "Excel Files|*.xlsx",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var grades = new System.Collections.Generic.List<Grade>();
                    
                    await using (var db = new AppDbContext())
                    {
                        grades = await db.Grades
                            .Include(g => g.Student)
                            .Include(g => g.Discipline)
                            .AsNoTracking()
                            .ToListAsync();
                    }

                    if (grades.Count == 0)
                    {
                        MessageBox.Show("Нет данных для экспорта", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    ExportService.ExportGradesToExcel(grades, saveDialog.FileName);
                    MessageBox.Show($"Экспортировано {grades.Count} записей в {saveDialog.FileName}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ExportStudents_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = $"Студенты_{DateTime.Now:yyyyMMdd_HHmmss}",
                    DefaultExt = ".xlsx",
                    Filter = "Excel Files|*.xlsx",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var students = new System.Collections.Generic.List<User>();
                    
                    await using (var db = new AppDbContext())
                    {
                        students = await db.Users
                            .Where(u => u.Role == "Student")
                            .AsNoTracking()
                            .ToListAsync();
                    }

                    if (students.Count == 0)
                    {
                        MessageBox.Show("Нет данных для экспорта", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    ExportService.ExportStudentsToExcel(students, saveDialog.FileName);
                    MessageBox.Show($"Экспортировано {students.Count} студентов в {saveDialog.FileName}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ExportSchedule_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = $"Расписание_{DateTime.Now:yyyyMMdd_HHmmss}",
                    DefaultExt = ".xlsx",
                    Filter = "Excel Files|*.xlsx",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var schedules = new System.Collections.Generic.List<Schedule>();
                    
                    await using (var db = new AppDbContext())
                    {
                        schedules = await db.Schedules
                            .Include(s => s.Discipline)
                            .Include(s => s.Group).ThenInclude(g => g!.Department)
                            .AsNoTracking()
                            .ToListAsync();
                    }

                    if (schedules.Count == 0)
                    {
                        MessageBox.Show("Нет данных для экспорта", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    ExportService.ExportScheduleToExcel(schedules, saveDialog.FileName);
                    MessageBox.Show($"Экспортировано {schedules.Count} занятий в {saveDialog.FileName}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ExportStats_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = $"Статистика_{DateTime.Now:yyyyMMdd_HHmmss}",
                    DefaultExt = ".xlsx",
                    Filter = "Excel Files|*.xlsx",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                };

                if (saveDialog.ShowDialog() == true)
                {
                    int facultyCount, departmentCount, groupCount, studentCount, teacherCount, disciplineCount;
                    
                    await using (var db = new AppDbContext())
                    {
                        facultyCount = await db.Faculties.CountAsync();
                        departmentCount = await db.Departments.CountAsync();
                        groupCount = await db.Groups.CountAsync();
                        studentCount = await db.Users.CountAsync(u => u.Role == "Student");
                        teacherCount = await db.Users.CountAsync(u => u.Role == "Teacher");
                        disciplineCount = await db.Disciplines.CountAsync();
                    }

                    using var workbook = new ClosedXML.Excel.XLWorkbook();
                    var ws = workbook.Worksheets.Add("Статистика системы");
                    
                    ws.Cell(1, 1).Value = "Статистика University System";
                    ws.Cell(1, 1).Style.Font.Bold = true;
                    ws.Cell(1, 1).Style.Font.FontSize = 16;
                    
                    ws.Cell(3, 1).Value = "Факультеты:";
                    ws.Cell(3, 2).Value = facultyCount;
                    ws.Cell(4, 1).Value = "Кафедры:";
                    ws.Cell(4, 2).Value = departmentCount;
                    ws.Cell(5, 1).Value = "Группы:";
                    ws.Cell(5, 2).Value = groupCount;
                    ws.Cell(6, 1).Value = "Студенты:";
                    ws.Cell(6, 2).Value = studentCount;
                    ws.Cell(7, 1).Value = "Преподаватели:";
                    ws.Cell(7, 2).Value = teacherCount;
                    ws.Cell(8, 1).Value = "Дисциплины:";
                    ws.Cell(8, 2).Value = disciplineCount;

                    workbook.SaveAs(saveDialog.FileName);
                    MessageBox.Show($"Статистика экспортирована в {saveDialog.FileName}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ExportGradesPdf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = $"Оценки_{DateTime.Now:yyyyMMdd_HHmmss}",
                    DefaultExt = ".pdf",
                    Filter = "PDF Files|*.pdf",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var grades = new System.Collections.Generic.List<Grade>();
                    
                    await using (var db = new AppDbContext())
                    {
                        grades = await db.Grades
                            .Include(g => g.Student)
                            .Include(g => g.Discipline)
                            .AsNoTracking()
                            .ToListAsync();
                    }

                    if (grades.Count == 0)
                    {
                        MessageBox.Show("Нет данных для экспорта", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    ExportService.ExportGradesToPdf(grades, saveDialog.FileName);
                    MessageBox.Show($"Экспортировано {grades.Count} записей в {saveDialog.FileName}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ExportSchedulePdf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = $"Расписание_{DateTime.Now:yyyyMMdd_HHmmss}",
                    DefaultExt = ".pdf",
                    Filter = "PDF Files|*.pdf",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var schedules = new System.Collections.Generic.List<Schedule>();
                    
                    await using (var db = new AppDbContext())
                    {
                        schedules = await db.Schedules
                            .Include(s => s.Discipline)
                            .Include(s => s.Group).ThenInclude(g => g!.Department)
                            .AsNoTracking()
                            .ToListAsync();
                    }

                    if (schedules.Count == 0)
                    {
                        MessageBox.Show("Нет данных для экспорта", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    ExportService.ExportScheduleToPdf(schedules, saveDialog.FileName);
                    MessageBox.Show($"Экспортировано {schedules.Count} занятий в {saveDialog.FileName}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ExportStatsPdf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = $"Статистика_{DateTime.Now:yyyyMMdd_HHmmss}",
                    DefaultExt = ".pdf",
                    Filter = "PDF Files|*.pdf",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                };

                if (saveDialog.ShowDialog() == true)
                {
                    int facultyCount, departmentCount, groupCount, studentCount, teacherCount, disciplineCount;
                    
                    await using (var db = new AppDbContext())
                    {
                        facultyCount = await db.Faculties.CountAsync();
                        departmentCount = await db.Departments.CountAsync();
                        groupCount = await db.Groups.CountAsync();
                        studentCount = await db.Users.CountAsync(u => u.Role == "Student");
                        teacherCount = await db.Users.CountAsync(u => u.Role == "Teacher");
                        disciplineCount = await db.Disciplines.CountAsync();
                    }

                    ExportService.ExportStatsToPdf(facultyCount, departmentCount, groupCount, studentCount, teacherCount, disciplineCount, saveDialog.FileName);
                    MessageBox.Show($"Статистика экспортирована в {saveDialog.FileName}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
