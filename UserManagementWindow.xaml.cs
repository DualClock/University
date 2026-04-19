using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using UniversitySystem.Data;
using UniversitySystem.Models;

namespace UniversitySystem;

public partial class UserManagementWindow : Window
{
    private List<User> _users;
    private List<Group> _groups;
    private User? _selectedUser;

    public UserManagementWindow()
    {
        InitializeComponent();
        LoadData();
    }

    private async void LoadData()
    {
        try
        {
            using var db = new AppDbContext();
            _users = await db.Users.Include(u => u.Group).AsNoTracking().ToListAsync();
            _groups = await db.Groups.AsNoTracking().ToListAsync();
            GroupFilterComboBox.ItemsSource = _groups;
            GroupComboBox.ItemsSource = _groups;
            RefreshDataGrid();
        }
        catch (Exception ex) { MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); }
    }

    private void RefreshDataGrid() => UserDataGrid.ItemsSource = _users;

    private void ApplyFilters()
    {
        var q = SearchTextBox.Text.Trim().ToLower();
        var roleFilter = (RoleFilterComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString();
        var groupId = GroupFilterComboBox.SelectedValue as int?;
        var result = _users.AsEnumerable();
        if (!string.IsNullOrEmpty(q)) result = result.Where(u => u.FullName.ToLower().Contains(q) || u.Login.ToLower().Contains(q));
        if (!string.IsNullOrEmpty(roleFilter)) result = result.Where(u => u.Role == roleFilter);
        if (groupId > 0) result = result.Where(u => u.GroupId == groupId);
        UserDataGrid.ItemsSource = result.ToList();
    }

    private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilters();
    private void RoleFilter_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyFilters();
    private void GroupFilter_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyFilters();

    private void AddUser_Click(object sender, RoutedEventArgs e)
    {
        _selectedUser = null;
        ClearForm();
        LoginTextBox.Focus();
    }

    private void EditUser_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedUser == null) { MessageBox.Show("Выберите пользователя", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
        LoginTextBox.Text = _selectedUser.Login;
        PasswordBox.Password = _selectedUser.Password;
        FullNameTextBox.Text = _selectedUser.FullName;
        EmailTextBox.Text = _selectedUser.Email;
        RoleComboBox.SelectedIndex = _selectedUser.Role switch { "Admin" => 2, "Teacher" => 1, _ => 0 };
        GroupComboBox.SelectedValue = _selectedUser.GroupId;
        LoginTextBox.Focus();
    }

    private async void DeleteUser_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedUser == null) { MessageBox.Show("Выберите пользователя", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
        var result = MessageBox.Show($"Удалить пользователя '{_selectedUser.FullName}'?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result == MessageBoxResult.Yes)
        {
            try
            {
                using var db = new AppDbContext();
                var u = await db.Users.FindAsync(_selectedUser.Id);
                if (u != null) { db.Users.Remove(u); await db.SaveChangesAsync(); }
                LoadData();
                ClearForm();
            }
            catch (Exception ex) { MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); }
        }
    }

    private async void SaveUser_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(LoginTextBox.Text)) { MessageBox.Show("Введите логин", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
        if (string.IsNullOrWhiteSpace(PasswordBox.Password) || PasswordBox.Password.Length < 4) { MessageBox.Show("Пароль минимум 4 символа", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
        if (string.IsNullOrWhiteSpace(FullNameTextBox.Text)) { MessageBox.Show("Введите ФИО", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
        var role = (RoleComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "Student";
        try
        {
            using var db = new AppDbContext();
            int? groupId = GroupComboBox.SelectedValue as int?;
            if (_selectedUser == null)
            {
                if (await db.Users.AnyAsync(u => u.Login == LoginTextBox.Text.Trim())) { MessageBox.Show("Логин уже существует", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
                db.Users.Add(new User { Login = LoginTextBox.Text.Trim(), Password = PasswordBox.Password, FullName = FullNameTextBox.Text.Trim(), Email = EmailTextBox.Text.Trim(), Role = role, GroupId = groupId });
            }
            else
            {
                if (await db.Users.AnyAsync(u => u.Login == LoginTextBox.Text.Trim() && u.Id != _selectedUser.Id)) { MessageBox.Show("Логин уже существует", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
                var u = await db.Users.FindAsync(_selectedUser.Id);
                if (u != null) { u.Login = LoginTextBox.Text.Trim(); u.Password = PasswordBox.Password; u.FullName = FullNameTextBox.Text.Trim(); u.Email = EmailTextBox.Text.Trim(); u.Role = role; u.GroupId = groupId; db.Users.Update(u); }
            }
            await db.SaveChangesAsync();
            LoadData();
            ClearForm();
            MessageBox.Show("Пользователь сохранён", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex) { MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); }
    }

    private void CancelUser_Click(object sender, RoutedEventArgs e) => ClearForm();

    private void ClearForm()
    {
        _selectedUser = null;
        LoginTextBox.Text = PasswordBox.Password = FullNameTextBox.Text = EmailTextBox.Text = "";
        if (GroupComboBox.Items.Count > 0) GroupComboBox.SelectedIndex = -1;
    }

    private void UserDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (UserDataGrid.SelectedItem is User u) _selectedUser = u;
    }
}
