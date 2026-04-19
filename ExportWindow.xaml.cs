using System;
using System.Linq;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using UniversitySystem.Data;
using UniversitySystem.Models;
using UniversitySystem.Services;

namespace UniversitySystem;

public partial class ExportWindow : Window
{
    public ExportWindow() { InitializeComponent(); }

    private async void Export_Click(object s, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog { Filter = "Excel Files|*.xlsx", FileName = "export.xlsx" };
        if (dialog.ShowDialog() != true) return;
        try
        {
            using var db = new AppDbContext();
            if (GradesRadio.IsChecked == true)
            {
                var grades = await db.Grades.Include(g => g.Student).Include(g => g.Discipline).AsNoTracking().ToListAsync();
                await ExportService.ExportGradesToExcelAsync(grades, dialog.FileName);
            }
            else if (StudentsRadio.IsChecked == true)
            {
                var students = await db.Users.Where(u => u.Role == "Student").Include(u => u.Group).AsNoTracking().ToListAsync();
                ExportService.ExportStudentsToExcel(students, dialog.FileName);
            }
            else if (GroupsRadio.IsChecked == true)
            {
                var groups = await db.Groups.Include(g => g.Department).AsNoTracking().ToListAsync();
                var exportGroups = groups.Select(g => new User { Id = g.Id, FullName = g.Name, Email = g.Department?.Name }).ToList();
                ExportService.ExportStudentsToExcel(exportGroups, dialog.FileName);
            }
            else if (ScheduleRadio.IsChecked == true)
            {
                var schedules = await db.Schedules.Include(s => s.Group).Include(s => s.Discipline).AsNoTracking().ToListAsync();
                var exportSched = schedules.Select((sc, i) => new Grade { Id = i + 1, Value = sc.DayOfWeek, Date = DateTime.UtcNow }).ToList();
                await ExportService.ExportGradesToExcelAsync(exportSched, dialog.FileName);
            }
            else if (DisciplinesRadio.IsChecked == true)
            {
                var disciplines = await db.Disciplines.AsNoTracking().ToListAsync();
                var exportDisc = disciplines.Select((d, i) => new Grade { Id = i + 1, Value = d.Credits, Date = DateTime.UtcNow }).ToList();
                await ExportService.ExportGradesToExcelAsync(exportDisc, dialog.FileName);
            }
            MessageBox.Show("Экспорт завершён", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex) { MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); }
    }

    private void Print_Click(object s, RoutedEventArgs e) { MessageBox.Show("Функция печати в разработке", "Информация"); }
}
