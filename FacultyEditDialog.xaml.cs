using System;
using System.Windows;
using UniversitySystem.Data;
using UniversitySystem.Models;

namespace UniversitySystem;

public partial class FacultyEditDialog : Window
{
    private Faculty _faculty;

    public FacultyEditDialog()
    {
        InitializeComponent();
        Title = "Добавление факультета";
        _faculty = new Faculty();
    }

    public FacultyEditDialog(Faculty faculty)
    {
        InitializeComponent();
        Title = "Редактирование факультета";
        _faculty = faculty;
        LoadData();
    }

    private void LoadData()
    {
        TxtName.Text = _faculty.Name;
        TxtCode.Text = _faculty.Code;
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        if (ValidateInput())
        {
            _faculty.Name = TxtName.Text.Trim();
            _faculty.Code = TxtCode.Text.Trim();

            try
            {
                using var context = new AppDbContext();
                if (_faculty.Id == 0)
                {
                    context.Faculties.Add(_faculty);
                }
                else
                {
                    context.Faculties.Update(_faculty);
                }
                context.SaveChanges();
                
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private bool ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(TxtName.Text))
        {
            MessageBox.Show("Введите название факультета", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (TxtName.Text.Length > 100)
        {
            MessageBox.Show("Название факультета не может быть длиннее 100 символов", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        return true;
    }
}