using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using UniversitySystem.Data;
using UniversitySystem.Models;
using GroupModel = UniversitySystem.Models.Group;

namespace UniversitySystem;

public partial class GroupManagementWindow : Window
{
    private List<GroupModel> _groups;
    private List<Department> _departments;
    private GroupModel? _selectedGroup;

    public GroupManagementWindow()
    {
        InitializeComponent();
        LoadData();
    }

    private async void LoadData()
    {
        try
        {
            using var db = new AppDbContext();
            
            _departments = await db.Departments
                .Include(d => d.Faculty)
                .AsNoTracking()
                .ToListAsync();

            _groups = await db.Groups
                .Include(g => g.Department)
                .AsNoTracking()
                .ToListAsync();

            var allOption = new Department { Id = 0, Name = "Все кафедры", Code = "" };
            var deptsWithAll = new List<Department> { allOption };
            deptsWithAll.AddRange(_departments);
            DepartmentFilterComboBox.ItemsSource = deptsWithAll;
            DepartmentFilterComboBox.DisplayMemberPath = "Name";
            DepartmentFilterComboBox.SelectedValuePath = "Id";
            DepartmentFilterComboBox.SelectedIndex = 0;

            DepartmentComboBox.ItemsSource = _departments;
            DepartmentComboBox.DisplayMemberPath = "Name";
            DepartmentComboBox.SelectedValuePath = "Id";

            RefreshDataGrid();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void RefreshDataGrid()
    {
        GroupDataGrid.ItemsSource = null;
        GroupDataGrid.ItemsSource = _groups;
    }

    private void FilterGroups()
    {
        if (DepartmentFilterComboBox.SelectedItem is Department dept && dept.Id > 0)
        {
            GroupDataGrid.ItemsSource = _groups.Where(g => g.DepartmentId == dept.Id).ToList();
        }
        else
        {
            GroupDataGrid.ItemsSource = _groups;
        }
    }

    private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        FilterGroups();
    }

    private void YearTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !Regex.IsMatch(e.Text, @"^\d+$");
    }

    private void AddGroup_Click(object sender, RoutedEventArgs e)
    {
        _selectedGroup = null;
        ClearForm();
        GroupNameTextBox.Focus();
        if (DepartmentComboBox.Items.Count > 0)
            DepartmentComboBox.SelectedIndex = 0;
    }

    private void EditGroup_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedGroup == null)
        {
            MessageBox.Show("Выберите группу для редактирования", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        GroupNameTextBox.Text = _selectedGroup.Name;
        GroupYearTextBox.Text = _selectedGroup.Year.ToString();
        DepartmentComboBox.SelectedValue = _selectedGroup.DepartmentId;
        GroupNameTextBox.Focus();
    }

    private async void DeleteGroup_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedGroup == null)
        {
            MessageBox.Show("Выберите группу для удаления", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        var result = MessageBox.Show($"Удалить группу '{_selectedGroup.Name}'?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result == MessageBoxResult.Yes)
        {
            try
            {
                using var db = new AppDbContext();
                var group = await db.Groups.FindAsync(_selectedGroup.Id);
                if (group != null)
                {
                    db.Groups.Remove(group);
                    await db.SaveChangesAsync();
                }
                LoadData();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления группы: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private async void SaveGroup_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(GroupNameTextBox.Text))
        {
            MessageBox.Show("Введите название группы", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (string.IsNullOrWhiteSpace(GroupYearTextBox.Text) || !int.TryParse(GroupYearTextBox.Text, out int year))
        {
            MessageBox.Show("Введите корректный год поступления", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (DepartmentComboBox.SelectedValue == null)
        {
            MessageBox.Show("Выберите кафедру", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        try
        {
            using var db = new AppDbContext();
            var departmentId = (int)DepartmentComboBox.SelectedValue;
            if (_selectedGroup == null)
            {
                if (await db.Groups.AnyAsync(g => g.Name == GroupNameTextBox.Text.Trim()))
                {
                    MessageBox.Show("Группа с таким названием уже существует", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                var newGroup = new GroupModel { Name = GroupNameTextBox.Text.Trim(), Year = year, DepartmentId = departmentId };
                db.Groups.Add(newGroup);
            }
            else
            {
                if (await db.Groups.AnyAsync(g => g.Name == GroupNameTextBox.Text.Trim() && g.Id != _selectedGroup.Id))
                {
                    MessageBox.Show("Группа с таким названием уже существует", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                var group = await db.Groups.FindAsync(_selectedGroup.Id);
                if (group != null)
                {
                    group.Name = GroupNameTextBox.Text.Trim();
                    group.Year = year;
                    group.DepartmentId = departmentId;
                    db.Groups.Update(group);
                }
            }
            await db.SaveChangesAsync();
            LoadData();
            ClearForm();
            MessageBox.Show("Группа сохранена успешно", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка сохранения группы: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CancelGroup_Click(object sender, RoutedEventArgs e) => ClearForm();

    private void ClearForm()
    {
        _selectedGroup = null;
        GroupNameTextBox.Text = "";
        GroupYearTextBox.Text = "";
        if (DepartmentComboBox.Items.Count > 0)
            DepartmentComboBox.SelectedIndex = -1;
    }

    private void GroupDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (GroupDataGrid.SelectedItem is GroupModel group) _selectedGroup = group;
    }
}
