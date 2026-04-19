using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using UniversitySystem.Services;
using UniversitySystem.Controls;
using UniversitySystem.Data;
using System.Threading.Tasks;

namespace UniversitySystem
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitializeUI();
            _ = LoadStatisticsAsync();
        }

        private async Task LoadStatisticsAsync()
        {
            try
            {
                await using var db = new AppDbContext();
                
                var facultyCount = await db.Faculties.CountAsync();
                var departmentCount = await db.Departments.CountAsync();
                var groupCount = await db.Groups.CountAsync();

                TxtFacultyCount.Text = facultyCount.ToString();
                TxtDepartmentCount.Text = departmentCount.ToString();
                TxtGroupCount.Text = groupCount.ToString();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading statistics: {ex.Message}");
            }
        }

        private void InitializeUI()
        {
            UserGreeting.Text = $"{RbacService.CurrentUserLogin} ({RbacService.CurrentUserRole})";
            SetupMenu();
            SetupQuickActions();
        }

        private void SetupMenu()
        {
            switch (RbacService.CurrentUserRole.ToLower())
            {
                case "admin":
                    AdminMenu.Visibility = Visibility.Visible;
                    TeacherMenu.Visibility = Visibility.Visible;
                    StudentMenu.Visibility = Visibility.Visible;
                    ReportsMenu.Visibility = Visibility.Visible;
                    break;
                case "teacher":
                    TeacherMenu.Visibility = Visibility.Visible;
                    StudentMenu.Visibility = Visibility.Visible;
                    ReportsMenu.Visibility = Visibility.Visible;
                    break;
                case "student":
                    StudentMenu.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void SetupQuickActions()
        {
            if (RbacService.CurrentUserRole.ToLower() == "student")
            {
                QuickAction1.Content = "Просмотреть мои оценки";
                QuickAction1.Click += (s, e) => ShowControl(new GradesViewControl());
                QuickAction2.Content = "Просмотреть расписание";
                QuickAction2.Click += (s, e) => ShowControl(new ScheduleControl());
            }
            else if (RbacService.CurrentUserRole.ToLower() == "teacher")
            {
                QuickAction1.Content = "Ввод оценок";
                QuickAction1.Click += (s, e) => ShowControl(new GradeManagementControl());
                QuickAction2.Content = "Просмотреть расписание";
                QuickAction2.Click += (s, e) => ShowControl(new ScheduleControl());
            }
            else
            {
                QuickAction1.Content = "Управление факультетами";
                QuickAction1.Click += (s, e) => ShowControl(new FacultyManagementControl());
                QuickAction2.Content = "Управление пользователями";
                QuickAction2.Click += (s, e) => ShowControl(new UserManagementControl());
            }
        }

        private void ShowControl(UserControl control)
        {
            ContentGrid.Children.Clear();
            ContentGrid.Children.Add(control);
        }

        private void OpenFacultyManagement(object sender, RoutedEventArgs e) => ShowControl(new FacultyManagementControl());
        private void OpenDepartmentManagement(object sender, RoutedEventArgs e) => ShowControl(new DepartmentManagementControl());
        private void OpenGroupManagement(object sender, RoutedEventArgs e) => ShowControl(new GroupManagementControl());
        private void OpenDisciplineManagement(object sender, RoutedEventArgs e) => ShowControl(new DisciplineManagementControl());
        private void OpenUserManagement(object sender, RoutedEventArgs e) => ShowControl(new UserManagementControl());
        private void OpenCurriculumManagement(object sender, RoutedEventArgs e) => ShowControl(new CurriculumManagementControl());
        private void OpenGradeManagement(object sender, RoutedEventArgs e) => ShowControl(new GradeManagementControl());
        private void OpenSchedule(object sender, RoutedEventArgs e) => ShowControl(new ScheduleControl());
        private void OpenExport(object sender, RoutedEventArgs e) => ShowControl(new ExportControl());
        private void OpenGradesView(object sender, RoutedEventArgs e) => ShowControl(new GradesViewControl());
        private void OpenReports(object sender, RoutedEventArgs e) => ShowControl(new ReportsControl());

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            AuthService.Logout();
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        }
    }
}