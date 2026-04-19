using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using UniversitySystem.Data;
using UniversitySystem.Models;
using UniversitySystem.Utils;

namespace UniversitySystem;

public partial class StudentPanel : UserControl
{
    private List<Grade> _allGrades = new();

    public StudentPanel()
    {
        InitializeComponent();
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        await LoadScheduleAsync();
        await LoadGradesAsync();
        await LoadNotificationsAsync();
        await LoadAcademicYearsAsync();
    }

    private async Task LoadScheduleAsync()
    {
        await using var db = new AppDbContext();
        var userId = RbacService.CurrentUserId;
        
        var user = await db.Users.Include(u => u.Group).FirstOrDefaultAsync(u => u.Id == userId);
        if (user?.GroupId == null)
        {
            DgSchedule.ItemsSource = Array.Empty<object>();
            return;
        }

        var schedule = await db.Schedules
            .Include(s => s.Discipline)
            .Where(s => s.GroupId == user.GroupId)
            .AsNoTracking()
            .ToListAsync();

        DgSchedule.ItemsSource = schedule;
    }

    private async Task LoadGradesAsync()
    {
        await using var db = new AppDbContext();
        var userId = RbacService.CurrentUserId;

        _allGrades = await db.Grades
            .Include(g => g.Discipline)
            .Where(g => g.StudentId == userId)
            .AsNoTracking()
            .ToListAsync();

        ApplyFilters();
    }

    private async Task LoadNotificationsAsync()
    {
        await using var db = new AppDbContext();
        var userId = RbacService.CurrentUserId;

        var notifications = await db.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .AsNoTracking()
            .ToListAsync();

        DgNotifications.ItemsSource = notifications;
    }

    private async Task LoadAcademicYearsAsync()
    {
        await using var db = new AppDbContext();
        var userId = RbacService.CurrentUserId;

        var years = await db.Grades
            .Where(g => g.StudentId == userId)
            .Select(g => g.AcademicYear)
            .Distinct()
            .OrderByDescending(y => y)
            .ToListAsync();

        CmbAcademicYearFilter.ItemsSource = years;
        if (years.Count > 0)
            CmbAcademicYearFilter.SelectedIndex = 0;
        
        CmbSemesterFilter.SelectedIndex = 0;
    }

    private void ApplyFilters()
    {
        if (_allGrades == null || _allGrades.Count == 0)
        {
            DgGrades.ItemsSource = null;
            TxtAverageGrade.Text = "0.00";
            return;
        }

        var filteredGrades = _allGrades.AsEnumerable();

        if (CmbSemesterFilter?.SelectedItem is ComboBoxItem semesterItem && semesterItem.Tag != null)
        {
            if (int.TryParse(semesterItem.Tag.ToString(), out int semester) && semester > 0)
            {
                filteredGrades = filteredGrades.Where(g => g.Semester == semester);
            }
        }

        if (CmbAcademicYearFilter?.SelectedItem != null)
        {
            var year = CmbAcademicYearFilter.SelectedItem.ToString();
            if (!string.IsNullOrEmpty(year))
            {
                filteredGrades = filteredGrades.Where(g => g.AcademicYear == year);
            }
        }

        var gradesList = filteredGrades.ToList();
        DgGrades.ItemsSource = gradesList;

        if (gradesList.Count > 0)
        {
            var average = gradesList.Average(g => g.Value);
            TxtAverageGrade.Text = average.ToString("F2");
        }
        else
        {
            TxtAverageGrade.Text = "0.00";
        }
    }

    private void CmbSemesterFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ApplyFilters();
    }

    private void CmbAcademicYearFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ApplyFilters();
    }

    private async void DgGrades_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (DgGrades.SelectedItem is Grade selectedGrade)
        {
            await LoadGradeHistoryAsync(selectedGrade.DisciplineId);
        }
    }

    private async Task LoadGradeHistoryAsync(int disciplineId)
    {
        await using var db = new AppDbContext();
        var userId = RbacService.CurrentUserId;

        var history = await db.Grades
            .Where(g => g.StudentId == userId && g.DisciplineId == disciplineId)
            .OrderByDescending(g => g.Date)
            .AsNoTracking()
            .ToListAsync();

        DgGradeHistory.ItemsSource = history;
        CardGradeHistory.Visibility = Visibility.Visible;
    }

    private void BtnCloseGradeHistory_Click(object sender, RoutedEventArgs e)
    {
        CardGradeHistory.Visibility = Visibility.Collapsed;
    }
}
