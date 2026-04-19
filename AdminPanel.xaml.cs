using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using UniversitySystem.Data;
using UniversitySystem.Models;

namespace UniversitySystem;

public partial class AdminPanel : UserControl
{
    public AdminPanel()
    {
        InitializeComponent();
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        await LoadFacultiesAsync();
        await LoadDepartmentsAsync();
        await LoadGroupsAsync();
        await LoadDisciplinesAsync();
        await LoadCurriculaAsync();
        await LoadUsersAsync();
    }

    private async Task LoadFacultiesAsync()
    {
        await using var db = new AppDbContext();
        var faculties = await db.Faculties.AsNoTracking().ToListAsync();
        DgFaculties.ItemsSource = faculties;
    }

    private async void BtnAddFaculty_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtFacultyName.Text) || string.IsNullOrWhiteSpace(TxtFacultyCode.Text))
        {
            MessageBox.Show("Заполните все поля", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            await using var db = new AppDbContext();
            var faculty = new Faculty
            {
                Name = TxtFacultyName.Text.Trim(),
                Code = TxtFacultyCode.Text.Trim()
            };
            db.Faculties.Add(faculty);
            await db.SaveChangesAsync();
            
            TxtFacultyName.Clear();
            TxtFacultyCode.Clear();
            await LoadFacultiesAsync();
            MessageBox.Show("Факультет добавлен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task LoadDepartmentsAsync()
    {
        await using var db = new AppDbContext();
        var departments = await db.Departments.Include(d => d.Faculty).AsNoTracking().ToListAsync();
        DgDepartments.ItemsSource = departments;

        var faculties = await db.Faculties.AsNoTracking().ToListAsync();
        CmbFacultyForDept.ItemsSource = faculties;
        CmbFacultyForDept.DisplayMemberPath = "Name";
        CmbFacultyForDept.SelectedValuePath = "Id";
    }

    private async void BtnAddDepartment_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtDepartmentName.Text) || string.IsNullOrWhiteSpace(TxtDepartmentCode.Text) || CmbFacultyForDept.SelectedValue == null)
        {
            MessageBox.Show("Заполните все поля", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            await using var db = new AppDbContext();
            var department = new Department
            {
                Name = TxtDepartmentName.Text.Trim(),
                Code = TxtDepartmentCode.Text.Trim(),
                FacultyId = (int)CmbFacultyForDept.SelectedValue
            };
            db.Departments.Add(department);
            await db.SaveChangesAsync();
            
            TxtDepartmentName.Clear();
            TxtDepartmentCode.Clear();
            CmbFacultyForDept.SelectedIndex = -1;
            await LoadDepartmentsAsync();
            MessageBox.Show("Кафедра добавлена", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task LoadGroupsAsync()
    {
        await using var db = new AppDbContext();
        var groups = await db.Groups.Include(g => g.Department).AsNoTracking().ToListAsync();
        DgGroups.ItemsSource = groups;

        var departments = await db.Departments.AsNoTracking().ToListAsync();
        CmbDepartmentForGroup.ItemsSource = departments;
        CmbDepartmentForGroup.DisplayMemberPath = "Name";
        CmbDepartmentForGroup.SelectedValuePath = "Id";
    }

    private async void BtnAddGroup_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtGroupName.Text) || string.IsNullOrWhiteSpace(TxtGroupYear.Text) || CmbDepartmentForGroup.SelectedValue == null)
        {
            MessageBox.Show("Заполните все поля", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!int.TryParse(TxtGroupYear.Text, out int year))
        {
            MessageBox.Show("Курс должен быть числом", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            await using var db = new AppDbContext();
            var group = new Group
            {
                Name = TxtGroupName.Text.Trim(),
                Year = year,
                DepartmentId = (int)CmbDepartmentForGroup.SelectedValue
            };
            db.Groups.Add(group);
            await db.SaveChangesAsync();
            
            TxtGroupName.Clear();
            TxtGroupYear.Clear();
            CmbDepartmentForGroup.SelectedIndex = -1;
            await LoadGroupsAsync();
            MessageBox.Show("Группа добавлена", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task LoadDisciplinesAsync()
    {
        await using var db = new AppDbContext();
        var disciplines = await db.Disciplines.AsNoTracking().ToListAsync();
        DgDisciplines.ItemsSource = disciplines;
    }

    private async void BtnAddDiscipline_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtDisciplineName.Text) || string.IsNullOrWhiteSpace(TxtDisciplineCode.Text))
        {
            MessageBox.Show("Заполните все поля", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            await using var db = new AppDbContext();
            var discipline = new Discipline
            {
                Name = TxtDisciplineName.Text.Trim(),
                Code = TxtDisciplineCode.Text.Trim(),
                LectureHours = 0,
                SeminarHours = 0,
                LabHours = 0,
                Credits = 0
            };
            db.Disciplines.Add(discipline);
            await db.SaveChangesAsync();
            
            TxtDisciplineName.Clear();
            TxtDisciplineCode.Clear();
            await LoadDisciplinesAsync();
            MessageBox.Show("Дисциплина добавлена", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task LoadCurriculaAsync()
    {
        await using var db = new AppDbContext();
        var curricula = await db.Curricula.Include(c => c.Group).Include(c => c.Discipline).AsNoTracking().ToListAsync();
        DgCurricula.ItemsSource = curricula;

        var groups = await db.Groups.AsNoTracking().ToListAsync();
        CmbGroupForCurriculum.ItemsSource = groups;
        CmbGroupForCurriculum.DisplayMemberPath = "Name";
        CmbGroupForCurriculum.SelectedValuePath = "Id";

        var disciplines = await db.Disciplines.AsNoTracking().ToListAsync();
        CmbDisciplineForCurriculum.ItemsSource = disciplines;
        CmbDisciplineForCurriculum.DisplayMemberPath = "Name";
        CmbDisciplineForCurriculum.SelectedValuePath = "Id";
    }

    private async void BtnAddCurriculum_Click(object sender, RoutedEventArgs e)
    {
        if (CmbGroupForCurriculum.SelectedValue == null || CmbDisciplineForCurriculum.SelectedValue == null || string.IsNullOrWhiteSpace(TxtSemester.Text))
        {
            MessageBox.Show("Заполните все поля", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!int.TryParse(TxtSemester.Text, out int semester))
        {
            MessageBox.Show("Семестр должен быть числом", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            await using var db = new AppDbContext();
            var curriculum = new Curriculum
            {
                GroupId = (int)CmbGroupForCurriculum.SelectedValue,
                DisciplineId = (int)CmbDisciplineForCurriculum.SelectedValue,
                Semester = semester,
                AcademicYear = TxtAcademicYear.Text.Trim()
            };
            db.Curricula.Add(curriculum);
            await db.SaveChangesAsync();
            
            CmbGroupForCurriculum.SelectedIndex = -1;
            CmbDisciplineForCurriculum.SelectedIndex = -1;
            TxtSemester.Clear();
            await LoadCurriculaAsync();
            MessageBox.Show("Запись добавлена в учебный план", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task LoadUsersAsync()
    {
        await using var db = new AppDbContext();
        var users = await db.Users.AsNoTracking().ToListAsync();
        DgUsers.ItemsSource = users;
    }

    private async void BtnAddUser_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtUserLogin.Text) || string.IsNullOrWhiteSpace(TxtUserPassword.Text) || string.IsNullOrWhiteSpace(TxtUserFullName.Text))
        {
            MessageBox.Show("Заполните все обязательные поля", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            await using var db = new AppDbContext();
            var role = "Student";
            if (CmbUserRole.SelectedItem is ComboBoxItem item)
            {
                role = item.Content.ToString() ?? "Student";
            }

            var user = new User
            {
                Login = TxtUserLogin.Text.Trim(),
                Password = TxtUserPassword.Text,
                FullName = TxtUserFullName.Text.Trim(),
                Role = role
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();
            
            TxtUserLogin.Clear();
            TxtUserPassword.Clear();
            TxtUserFullName.Clear();
            await LoadUsersAsync();
            MessageBox.Show("Пользователь добавлен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}