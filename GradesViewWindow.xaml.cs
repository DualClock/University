using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using UniversitySystem.Data;
using UniversitySystem.Models;
using UniversitySystem.Services;

namespace UniversitySystem;

public partial class GradesViewWindow : Window
{
    private List<User> _students = new();

    public GradesViewWindow()
    {
        InitializeComponent();
        LoadStudents();
    }

    private async void LoadStudents()
    {
        using var db = new AppDbContext();
        _students = await db.Users.Where(u => u.Role == "Student").Include(u => u.Group).AsNoTracking().ToListAsync();
        StudentComboBox.ItemsSource = _students;
    }

    private async void Student_SelectionChanged(object s, SelectionChangedEventArgs e)
    {
        if (StudentComboBox.SelectedValue is int studentId)
        {
            using var db = new AppDbContext();
            var student = await db.Users.FindAsync(studentId);
            var grades = await db.Grades.Where(g => g.StudentId == studentId).Include(g => g.Discipline).AsNoTracking().ToListAsync();
            GradesDataGrid.ItemsSource = grades;
            StudentNameText.Text = student?.FullName ?? "";
            var avg = grades.Any() ? grades.Average(g => g.Value) : 0;
            AverageText.Text = $"Средний балл: {avg:F2}";
            TotalAverageText.Text = avg.ToString("F2");
        }
    }

    private async void ExportToExcel_Click(object s, RoutedEventArgs e)
    {
        if (StudentComboBox.SelectedValue is int studentId)
        {
            using var db = new AppDbContext();
            var grades = await db.Grades.Where(g => g.StudentId == studentId).Include(g => g.Discipline).AsNoTracking().ToListAsync();
            if (grades.Count == 0) { MessageBox.Show("Нет оценок для экспорта"); return; }
            var dialog = new SaveFileDialog { Filter = "Excel Files|*.xlsx", FileName = "grades.xlsx" };
            if (dialog.ShowDialog() == true)
            {
                await ExportService.ExportGradesToExcelAsync(grades, dialog.FileName);
                MessageBox.Show("Экспорт завершён", "Успех");
            }
        }
        else MessageBox.Show("Выберите студента");
    }
}
