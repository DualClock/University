using System;
using System.Windows;
using UniversitySystem.Data;
using UniversitySystem.Models;
using UniversitySystem.Services;

namespace UniversitySystem;

public partial class MainMenuWindow : Window
{
    public MainMenuWindow()
    {
        InitializeComponent();
        LoadUserInfo();
        SetupPermissions();
    }

    private void LoadUserInfo()
    {
        if (RbacService.CurrentUserId > 0)
        {
            using var context = new AppDbContext();
            var user = context.Users.Find(RbacService.CurrentUserId);
            if (user != null)
            {
                TxtUserInfo.Text = $"Пользователь: {user.FullName} ({user.Role})";
            }
        }
    }

    private void SetupPermissions()
    {
        var role = RbacService.CurrentUserRole;
        
        if (role != "Admin")
        {
            BtnManageFaculties.Visibility = Visibility.Collapsed;
            BtnManageDepartments.Visibility = Visibility.Collapsed;
            BtnManageGroups.Visibility = Visibility.Collapsed;
            BtnManageDisciplines.Visibility = Visibility.Collapsed;
            BtnManageUsers.Visibility = Visibility.Collapsed;
            BtnManageCurriculum.Visibility = Visibility.Collapsed;
        }

        if (role != "Teacher" && role != "Admin")
        {
            BtnManageGrades.Visibility = Visibility.Collapsed;
            BtnViewSchedule.Visibility = Visibility.Collapsed;
            BtnViewGrades.Visibility = Visibility.Collapsed;
        }
    }

    private void BtnManageFaculties_Click(object sender, RoutedEventArgs e)
    {
        var window = new FacultyManagementWindow();
        window.Owner = this;
        window.ShowDialog();
    }

    private void BtnManageDepartments_Click(object sender, RoutedEventArgs e)
    {
        var window = new DepartmentManagementWindow();
        window.Owner = this;
        window.ShowDialog();
    }

    private void BtnManageGroups_Click(object sender, RoutedEventArgs e)
    {
        var window = new GroupManagementWindow();
        window.Owner = this;
        window.ShowDialog();
    }

    private void BtnManageDisciplines_Click(object sender, RoutedEventArgs e)
    {
        var window = new DisciplineManagementWindow();
        window.Owner = this;
        window.ShowDialog();
    }

    private void BtnManageUsers_Click(object sender, RoutedEventArgs e)
    {
        var window = new UserManagementWindow();
        window.Owner = this;
        window.ShowDialog();
    }

    private void BtnManageCurriculum_Click(object sender, RoutedEventArgs e)
    {
        var window = new CurriculumManagementWindow();
        window.Owner = this;
        window.ShowDialog();
    }

    private void BtnManageGrades_Click(object sender, RoutedEventArgs e)
    {
        var window = new GradeManagementWindow();
        window.Owner = this;
        window.ShowDialog();
    }

    private void BtnViewSchedule_Click(object sender, RoutedEventArgs e)
    {
        var window = new ScheduleWindow();
        window.Owner = this;
        window.ShowDialog();
    }

    private void BtnViewGrades_Click(object sender, RoutedEventArgs e)
    {
        var window = new GradesViewWindow();
        window.Owner = this;
        window.ShowDialog();
    }

    private void BtnReports_Click(object sender, RoutedEventArgs e)
    {
        var window = new ReportsWindow();
        window.Owner = this;
        window.ShowDialog();
    }

    private void BtnExport_Click(object sender, RoutedEventArgs e)
    {
        var window = new ExportWindow();
        window.Owner = this;
        window.ShowDialog();
    }

    private void BtnLogout_Click(object sender, RoutedEventArgs e)
    {
        RbacService.Logout();
        Close();
    }
}