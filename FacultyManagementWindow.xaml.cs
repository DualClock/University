using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using UniversitySystem.Data;
using UniversitySystem.Models;

namespace UniversitySystem;

public partial class FacultyManagementWindow : Window
{
    private List<Faculty> _faculties;
    private Faculty _selectedFaculty;

    public FacultyManagementWindow()
    {
        InitializeComponent();
        LoadFaculties();
    }

    private async void LoadFaculties()
    {
        try
        {
            using var db = new AppDbContext();
            _faculties = await db.Faculties.ToListAsync();
            FacultyDataGrid.ItemsSource = _faculties;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки факультетов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void AddFaculty_Click(object sender, RoutedEventArgs e)
    {
        ClearForm();
        FacultyNameTextBox.Focus();
    }

    private void EditFaculty_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedFaculty == null)
        {
            MessageBox.Show("Выберите факультет для редактирования", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        FacultyNameTextBox.Text = _selectedFaculty.Name;
        ClearForm();
        FacultyNameTextBox.Focus();
    }

    private async void DeleteFaculty_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedFaculty == null)
        {
            MessageBox.Show("Выберите факультет для удаления", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show($"Удалить факультет '{_selectedFaculty.Name}'?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result == MessageBoxResult.Yes)
        {
            try
            {
                using var db = new AppDbContext();
                db.Faculties.Remove(_selectedFaculty);
                await db.SaveChangesAsync();
                LoadFaculties();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления факультета: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private async void SaveFaculty_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(FacultyNameTextBox.Text))
        {
            MessageBox.Show("Введите название факультета", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            using var db = new AppDbContext();

            if (_selectedFaculty == null)
            {
                var newFaculty = new Faculty
                {
                    Name = FacultyNameTextBox.Text.Trim(),
                    Code = FacultyNameTextBox.Text.Trim().Substring(0, Math.Min(3, FacultyNameTextBox.Text.Trim().Length)).ToUpper()
                };
                db.Faculties.Add(newFaculty);
            }
            else
            {
                _selectedFaculty.Name = FacultyNameTextBox.Text.Trim();
                _selectedFaculty.Code = FacultyNameTextBox.Text.Trim().Substring(0, Math.Min(3, FacultyNameTextBox.Text.Trim().Length)).ToUpper();
                db.Faculties.Update(_selectedFaculty);
            }

            await db.SaveChangesAsync();
            LoadFaculties();
            ClearForm();
            MessageBox.Show("Факультет сохранен успешно", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка сохранения факультета: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CancelFaculty_Click(object sender, RoutedEventArgs e)
    {
        ClearForm();
    }

    private void ClearForm()
    {
        _selectedFaculty = null;
        FacultyNameTextBox.Text = "";
    }

    private void FacultyDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (FacultyDataGrid.SelectedItem is Faculty faculty)
        {
            _selectedFaculty = faculty;
        }
    }
}