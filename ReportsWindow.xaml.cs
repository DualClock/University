using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using UniversitySystem.Data;
using UniversitySystem.Models;
using UniversitySystem.Services;

namespace UniversitySystem;

public partial class ReportsWindow : Window
{
    public ReportsWindow() { InitializeComponent(); LoadGroups(); }

    private async void LoadGroups()
    {
        using var db = new AppDbContext();
        var groups = await db.Groups.AsNoTracking().ToListAsync();
        GroupComboBox.ItemsSource = groups;
        DebtorsGroupComboBox.ItemsSource = groups;
    }

    private async void ShowStats_Click(object s, RoutedEventArgs e)
    {
        if (GroupComboBox.SelectedValue is int groupId)
        {
            using var db = new AppDbContext();
            var students = await db.Users.Where(u => u.GroupId == groupId).ToListAsync();
            var grades = await db.Grades.Where(g => students.Select(st => st.Id).Contains(g.StudentId)).ToListAsync();
            TotalStudentsText.Text = students.Count.ToString();
            if (grades.Count > 0)
            {
                AvgGradeText.Text = grades.Average(g => g.Value).ToString("F2");
                ExcellentText.Text = grades.Count(g => g.Value == 5).ToString();
                GoodText.Text = grades.Count(g => g.Value == 4).ToString();
                SatisfactoryText.Text = grades.Count(g => g.Value == 3).ToString();
                FailText.Text = grades.Count(g => g.Value == 2).ToString();
            }
            else { AvgGradeText.Text = "0"; ExcellentText.Text = GoodText.Text = SatisfactoryText.Text = FailText.Text = "0"; }
        }
    }

    private async void ExportStats_Click(object s, RoutedEventArgs e)
    {
        var d = new SaveFileDialog { Filter = "Excel|*.xlsx", FileName = "stats.xlsx" };
        if (d.ShowDialog() == true) MessageBox.Show("Экспорт статистики", "Информация");
    }

    private async void FindDebtors_Click(object s, RoutedEventArgs e)
    {
        if (DebtorsGroupComboBox.SelectedValue is int groupId)
        {
            using var db = new AppDbContext();
            var students = await db.Users.Where(u => u.GroupId == groupId).ToListAsync();
            var debtors = await db.Grades.Include(g => g.Student).Include(g => g.Discipline)
                .Where(g => students.Select(st => st.Id).Contains(g.StudentId) && g.Value < 3)
                .GroupBy(g => new { g.StudentId, g.DisciplineId })
                .Select(g => new { StudentName = g.First().Student.FullName, DisciplineName = g.First().Discipline.Name, DebtCount = g.Count() })
                .ToListAsync();
            DebtorsDataGrid.ItemsSource = debtors;
        }
    }

    private void Group_SelectionChanged(object s, SelectionChangedEventArgs e) { }
}
