using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using UniversitySystem.Data;
using UniversitySystem.Models;

namespace UniversitySystem;

public partial class DepartmentManagementWindow : Window
{
    private List<Department> _departments;
    private List<Faculty> _faculties;
    private Department? _selectedDepartment;

    public DepartmentManagementWindow()
    {
        InitializeComponent();
        LoadData();
    }

    private async void LoadData()
    {
        try
        {
            using var db = new AppDbContext();
            
            _faculties = await db.Faculties.AsNoTracking().ToListAsync();
            
            _departments = await db.Departments
                .Include(d => d.Faculty)
                .AsNoTracking()
                .ToListAsync();

            FacultyFilterComboBox.ItemsSource = _faculties;
            FacultyFilterComboBox.DisplayMemberPath = "Name";
            FacultyFilterComboBox.SelectedValuePath = "Id";
            
            var allOption = new Faculty { Id = 0, Name = "Все факультеты" };
            var facultiesWithAll = new List<Faculty> { allOption };
            facultiesWithAll.AddRange(_faculties);
            FacultyFilterComboBox.ItemsSource = facultiesWithAll;
            FacultyFilterComboBox.SelectedIndex = 0;

            FacultyComboBox.ItemsSource = _faculties;
            FacultyComboBox.DisplayMemberPath = "Name";
            FacultyComboBox.SelectedValuePath = "Id";

            RefreshDataGrid();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void RefreshDataGrid()
    {
        DepartmentDataGrid.ItemsSource = null;
        DepartmentDataGrid.ItemsSource = _departments;
    }

    private void FilterDepartments()
    {
        if (FacultyFilterComboBox.SelectedItem is Faculty selectedFaculty && selectedFaculty.Id > 0)
        {
            DepartmentDataGrid.ItemsSource = _departments.Where(d => d.FacultyId == selectedFaculty.Id).ToList();
        }
        else
        {
            DepartmentDataGrid.ItemsSource = _departments;
        }
    }

    private void FacultyFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        FilterDepartments();
    }

    private void AddDepartment_Click(object sender, RoutedEventArgs e)
    {
        _selectedDepartment = null;
        ClearForm();
        DepartmentNameTextBox.Focus();
        
        if (FacultyComboBox.Items.Count > 0)
            FacultyComboBox.SelectedIndex = 0;
    }

    private void EditDepartment_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedDepartment == null)
        {
            MessageBox.Show("Выберите кафедру для редактирования", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        DepartmentNameTextBox.Text = _selectedDepartment.Name;
        DepartmentCodeTextBox.Text = _selectedDepartment.Code;
        FacultyComboBox.SelectedValue = _selectedDepartment.FacultyId;
        DepartmentNameTextBox.Focus();
    }

    private async void DeleteDepartment_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedDepartment == null)
        {
            MessageBox.Show("Выберите кафедру для удаления", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show($"Удалить кафедру '{_selectedDepartment.Name}'?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result == MessageBoxResult.Yes)
        {
            try
            {
                using var db = new AppDbContext();
                var department = await db.Departments.FindAsync(_selectedDepartment.Id);
                if (department != null)
                {
                    db.Departments.Remove(department);
                    await db.SaveChangesAsync();
                }
                LoadData();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления кафедры: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private async void SaveDepartment_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(DepartmentNameTextBox.Text))
        {
            MessageBox.Show("Введите название кафедры", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(DepartmentCodeTextBox.Text))
        {
            MessageBox.Show("Введите код кафедры", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (FacultyComboBox.SelectedValue == null)
        {
            MessageBox.Show("Выберите факультет", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            using var db = new AppDbContext();
            var facultyId = (int)FacultyComboBox.SelectedValue;

            if (_selectedDepartment == null)
            {
                if (await db.Departments.AnyAsync(d => d.Code == DepartmentCodeTextBox.Text.Trim()))
                {
                    MessageBox.Show("Кафедра с таким кодом уже существует", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var newDepartment = new Department
                {
                    Name = DepartmentNameTextBox.Text.Trim(),
                    Code = DepartmentCodeTextBox.Text.Trim().ToUpper(),
                    FacultyId = facultyId
                };
                db.Departments.Add(newDepartment);
            }
            else
            {
                if (await db.Departments.AnyAsync(d => d.Code == DepartmentCodeTextBox.Text.Trim() && d.Id != _selectedDepartment.Id))
                {
                    MessageBox.Show("Кафедра с таким кодом уже существует", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var department = await db.Departments.FindAsync(_selectedDepartment.Id);
                if (department != null)
                {
                    department.Name = DepartmentNameTextBox.Text.Trim();
                    department.Code = DepartmentCodeTextBox.Text.Trim().ToUpper();
                    department.FacultyId = facultyId;
                    db.Departments.Update(department);
                }
            }

            await db.SaveChangesAsync();
            LoadData();
            ClearForm();
            MessageBox.Show("Кафедра сохранена успешно", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка сохранения кафедры: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CancelDepartment_Click(object sender, RoutedEventArgs e)
    {
        ClearForm();
    }

    private void ClearForm()
    {
        _selectedDepartment = null;
        DepartmentNameTextBox.Text = "";
        DepartmentCodeTextBox.Text = "";
        if (FacultyComboBox.Items.Count > 0)
            FacultyComboBox.SelectedIndex = -1;
    }

    private void DepartmentDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DepartmentDataGrid.SelectedItem is Department department)
        {
            _selectedDepartment = department;
        }
    }
}
