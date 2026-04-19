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
                    await using (var db = new AppDbContext())
                    {
                        var grades = await db.Grades
                            .Include(g => g.Student)
                            .Include(g => g.Discipline)
                            .AsNoTracking()
                            .ToListAsync();

                        if (grades.Count == 0)
                        {
                            MessageBox.Show("Нет данных для экспорта", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }

                        using var workbook = new ClosedXML.Excel.XLWorkbook();
                        var ws = workbook.Worksheets.Add("Оценки");

                        
                        ws.Cell(1, 1).Value = "ЖУРНАЛ ОЦЕНОК";
                        ws.Cell(1, 1).Style.Font.Bold = true;
                        ws.Cell(1, 1).Style.Font.FontSize = 14;
                        ws.Range(1, 1, 1, 8).Merge();

                       
                        var headers = new[] { "#", "Студент", "Дисциплина", "Тип", "Оценка", "Дата", "Семестр", "Год" };
                        for (int i = 0; i < headers.Length; i++)
                        {
                            ws.Cell(3, i + 1).Value = headers[i];
                            ws.Cell(3, i + 1).Style.Font.Bold = true;
                            ws.Cell(3, i + 1).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGray;
                            ws.Cell(3, i + 1).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                        }

                        for (int row = 0; row < grades.Count; row++)
                        {
                            try
                            {
                                var g = grades[row];
                                ws.Cell(row + 4, 1).Value = row + 1;
                                ws.Cell(row + 4, 2).Value = g.Student?.FullName ?? $"[Студент #{g.StudentId}]";
                                ws.Cell(row + 4, 3).Value = g.Discipline?.Name ?? $"[Дисциплина #{g.DisciplineId}]";
                                ws.Cell(row + 4, 4).Value = g.Type ?? "";
                                ws.Cell(row + 4, 5).Value = g.Value;
                                ws.Cell(row + 4, 6).Value = g.Date;
                                ws.Cell(row + 4, 6).Style.DateFormat.Format = "dd.MM.yyyy";
                                ws.Cell(row + 4, 7).Value = g.Semester;
                                ws.Cell(row + 4, 8).Value = g.AcademicYear ?? "";
                            }
                            catch (Exception rowEx)
                            {
                                throw new Exception($"Ошибка в записи оценки #{row + 1}: {rowEx.Message}", rowEx);
                            }
                        }

                        try
                        {
                            ws.Columns().AdjustToContents();
                        }
                        catch
                        {
                            ws.Column(1).Width = 8;
                            ws.Column(2).Width = 30;
                            ws.Column(3).Width = 30;
                            ws.Column(4).Width = 16;
                            ws.Column(5).Width = 10;
                            ws.Column(6).Width = 14;
                            ws.Column(7).Width = 10;
                            ws.Column(8).Width = 14;
                        }
                        workbook.SaveAs(saveDialog.FileName);
                    }

                    MessageBox.Show($"Экспортировано в {saveDialog.FileName}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта: {ex}\n\nInner: {ex.InnerException?.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    await using (var db = new AppDbContext())
                    {
                        var students = await db.Users
                            .Where(u => u.Role == "Student")
                            .Include(u => u.Group)
                            .Include(u => u.Department)
                            .AsNoTracking()
                            .ToListAsync();

                        if (students.Count == 0)
                        {
                            MessageBox.Show("Нет данных для экспорта", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }

                        using var workbook = new ClosedXML.Excel.XLWorkbook();
                        var ws = workbook.Worksheets.Add("Студенты");

                        
                        ws.Cell(1, 1).Value = "СПИСОК СТУДЕНТОВ";
                        ws.Cell(1, 1).Style.Font.Bold = true;
                        ws.Cell(1, 1).Style.Font.FontSize = 14;
                        ws.Range(1, 1, 1, 7).Merge();

                        
                        var headers = new[] { "ID", "Логин", "ФИО", "Email", "Роль", "Группа", "Кафедра" };
                        for (int i = 0; i < headers.Length; i++)
                        {
                            ws.Cell(3, i + 1).Value = headers[i];
                            ws.Cell(3, i + 1).Style.Font.Bold = true;
                            ws.Cell(3, i + 1).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGray;
                            ws.Cell(3, i + 1).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                        }

                        for (int row = 0; row < students.Count; row++)
                        {
                            var s = students[row];
                            ws.Cell(row + 4, 1).Value = s.Id;
                            ws.Cell(row + 4, 2).Value = s.Login ?? "";
                            ws.Cell(row + 4, 3).Value = s.FullName ?? "";
                            ws.Cell(row + 4, 4).Value = s.Email ?? "N/A";
                            ws.Cell(row + 4, 5).Value = s.Role ?? "";
                            ws.Cell(row + 4, 6).Value = s.Group?.Name ?? "N/A";
                            ws.Cell(row + 4, 7).Value = s.Department?.Name ?? "N/A";
                        }

                        try
                        {
                            ws.Columns().AdjustToContents();
                        }
                        catch
                        {
                            ws.Column(1).Width = 8;
                            ws.Column(2).Width = 20;
                            ws.Column(3).Width = 30;
                            ws.Column(4).Width = 28;
                            ws.Column(5).Width = 14;
                            ws.Column(6).Width = 18;
                            ws.Column(7).Width = 24;
                        }
                        workbook.SaveAs(saveDialog.FileName);
                    }

                    MessageBox.Show($"Экспортировано в {saveDialog.FileName}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
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
                    var schedules = await LoadSchedulesForExportAsync();

                    if (schedules.Count == 0)
                    {
                        MessageBox.Show("Нет данных для экспорта", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    using var workbook = new ClosedXML.Excel.XLWorkbook();
                    var ws = workbook.Worksheets.Add("Расписание");

                    ws.Cell(1, 1).Value = "РАСПИСАНИЕ ЗАНЯТИЙ";
                    ws.Cell(1, 1).Style.Font.Bold = true;
                    ws.Cell(1, 1).Style.Font.FontSize = 14;
                    ws.Range(1, 1, 1, 8).Merge();

                    var headers = new[] { "#", "День", "Начало", "Конец", "Дисциплина", "Группа", "Аудитория", "Тип" };
                    for (int i = 0; i < headers.Length; i++)
                    {
                        ws.Cell(3, i + 1).Value = headers[i];
                        ws.Cell(3, i + 1).Style.Font.Bold = true;
                        ws.Cell(3, i + 1).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGray;
                        ws.Cell(3, i + 1).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                    }

                    string[] days = { "", "Понедельник", "Вторник", "Среда", "Четверг", "Пятница", "Суббота" };

                    for (int row = 0; row < schedules.Count; row++)
                    {
                        var s = schedules[row];
                        ws.Cell(row + 4, 1).Value = row + 1;
                        ws.Cell(row + 4, 2).Value = s.DayOfWeek > 0 && s.DayOfWeek < days.Length ? days[s.DayOfWeek] : "";
                        ws.Cell(row + 4, 3).Value = s.StartTime.ToString("HH:mm");
                        ws.Cell(row + 4, 4).Value = s.EndTime.ToString("HH:mm");
                        ws.Cell(row + 4, 5).Value = s.Discipline?.Name ?? $"[Дисциплина #{s.DisciplineId}]";
                        ws.Cell(row + 4, 6).Value = s.Group?.Name ?? $"[Группа #{s.GroupId}]";
                        ws.Cell(row + 4, 7).Value = string.IsNullOrWhiteSpace(s.Room) ? "N/A" : s.Room;
                        ws.Cell(row + 4, 8).Value = string.IsNullOrWhiteSpace(s.Type) ? "Lecture" : s.Type;
                    }

                    try
                    {
                        ws.Columns().AdjustToContents();
                    }
                    catch
                    {
                        ws.Column(1).Width = 8;
                        ws.Column(2).Width = 16;
                        ws.Column(3).Width = 10;
                        ws.Column(4).Width = 10;
                        ws.Column(5).Width = 30;
                        ws.Column(6).Width = 18;
                        ws.Column(7).Width = 14;
                        ws.Column(8).Width = 14;
                    }

                    workbook.SaveAs(saveDialog.FileName);
                    MessageBox.Show($"Расписание экспортировано в {saveDialog.FileName}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static async Task<List<Schedule>> LoadSchedulesForExportAsync()
        {
            await using var db = new AppDbContext();
            return await db.Schedules
                .Include(s => s.Discipline)
                .Include(s => s.Group).ThenInclude(g => g!.Department)
                .AsNoTracking()
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.StartTime)
                .ToListAsync();
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
                    await using (var db = new AppDbContext())
                    {
                        var grades = await db.Grades
                            .Include(g => g.Student)
                            .Include(g => g.Discipline)
                            .AsNoTracking()
                            .ToListAsync();

                        if (grades.Count == 0)
                        {
                            MessageBox.Show("Нет данных для экспорта", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }

                        ExportService.ExportGradesToPdf(grades, saveDialog.FileName);
                    }

                    MessageBox.Show($"Экспортировано в {saveDialog.FileName}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
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
                    await using (var db = new AppDbContext())
                    {
                        var schedules = await db.Schedules
                            .Include(s => s.Discipline)
                            .Include(s => s.Group).ThenInclude(g => g!.Department)
                            .AsNoTracking()
                            .ToListAsync();

                        if (schedules.Count == 0)
                        {
                            MessageBox.Show("Нет данных для экспорта", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }

                        ExportService.ExportScheduleToPdf(schedules, saveDialog.FileName);
                    }

                    MessageBox.Show($"Экспортировано в {saveDialog.FileName}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
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
