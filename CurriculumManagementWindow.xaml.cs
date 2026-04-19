using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using UniversitySystem.Data;
using UniversitySystem.Models;

namespace UniversitySystem;

public partial class CurriculumManagementWindow : Window
{
    private List<Curriculum> _curricula;
    private List<Group> _groups;
    private List<Discipline> _disciplines;
    private Curriculum? _selected;

    public CurriculumManagementWindow()
    {
        InitializeComponent();
        LoadData();
    }

    private async void LoadData()
    {
        try
        {
            using var db = new AppDbContext();
            _groups = await db.Groups.AsNoTracking().ToListAsync();
            _disciplines = await db.Disciplines.AsNoTracking().ToListAsync();
            _curricula = await db.Curricula.Include(c => c.Group).Include(c => c.Discipline).AsNoTracking().ToListAsync();
            GroupFilterComboBox.ItemsSource = _groups;
            GroupComboBox.ItemsSource = _groups;
            DisciplineComboBox.ItemsSource = _disciplines;
            RefreshDataGrid();
        }
        catch (Exception ex) { MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); }
    }

    private void RefreshDataGrid() => CurriculumDataGrid.ItemsSource = _curricula;

    private void ApplyFilters()
    {
        var groupId = GroupFilterComboBox.SelectedValue as int?;
        var semesterStr = (SemesterFilterComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
        int? semester = int.TryParse(semesterStr, out int s) ? s : null;
        var year = YearFilterTextBox.Text.Trim();
        var result = _curricula.AsEnumerable();
        if (groupId > 0) result = result.Where(c => c.GroupId == groupId);
        if (semester > 0) result = result.Where(c => c.Semester == semester);
        if (!string.IsNullOrEmpty(year)) result = result.Where(c => c.AcademicYear.Contains(year));
        CurriculumDataGrid.ItemsSource = result.ToList();
    }

    private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyFilters();
    private void Filter_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilters();

    private void AddCurriculum_Click(object sender, RoutedEventArgs e)
    {
        _selected = null;
        ClearForm();
    }

    private async void DeleteCurriculum_Click(object sender, RoutedEventArgs e)
    {
        if (_selected == null) { MessageBox.Show("Выберите запись", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
        var result = MessageBox.Show("Удалить запись?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result == MessageBoxResult.Yes)
        {
            try
            {
                using var db = new AppDbContext();
                var c = await db.Curricula.FindAsync(_selected.Id);
                if (c != null) { db.Curricula.Remove(c); await db.SaveChangesAsync(); }
                LoadData();
                ClearForm();
            }
            catch (Exception ex) { MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); }
        }
    }

    private async void SaveCurriculum_Click(object sender, RoutedEventArgs e)
    {
        if (GroupComboBox.SelectedValue == null) { MessageBox.Show("Выберите группу", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
        if (DisciplineComboBox.SelectedValue == null) { MessageBox.Show("Выберите дисциплину", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
        var semesterStr = (SemesterComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
        if (!int.TryParse(semesterStr, out int semester)) { MessageBox.Show("Выберите семестр", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
        if (string.IsNullOrWhiteSpace(YearTextBox.Text)) { MessageBox.Show("Введите учебный год", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
        try
        {
            using var db = new AppDbContext();
            int groupId = (int)GroupComboBox.SelectedValue;
            int disciplineId = (int)DisciplineComboBox.SelectedValue;
            if (_selected == null)
            {
                if (await db.Curricula.AnyAsync(c => c.GroupId == groupId && c.DisciplineId == disciplineId && c.Semester == semester && c.AcademicYear == YearTextBox.Text.Trim())) { MessageBox.Show("Запись уже существует", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
                db.Curricula.Add(new Curriculum { GroupId = groupId, DisciplineId = disciplineId, Semester = semester, AcademicYear = YearTextBox.Text.Trim() });
            }
            else
            {
                var c = await db.Curricula.FindAsync(_selected.Id);
                if (c != null) { c.GroupId = groupId; c.DisciplineId = disciplineId; c.Semester = semester; c.AcademicYear = YearTextBox.Text.Trim(); db.Curricula.Update(c); }
            }
            await db.SaveChangesAsync();
            LoadData();
            ClearForm();
            MessageBox.Show("Сохранено", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex) { MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); }
    }

    private void CancelCurriculum_Click(object sender, RoutedEventArgs e) => ClearForm();

    private void ClearForm()
    {
        _selected = null;
        GroupComboBox.SelectedIndex = DisciplineComboBox.SelectedIndex = -1;
        SemesterComboBox.SelectedIndex = 0;
        YearTextBox.Text = "2025/2026";
    }

    private void CurriculumDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CurriculumDataGrid.SelectedItem is Curriculum c) _selected = c;
    }
}
