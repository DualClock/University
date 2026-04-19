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

namespace UniversitySystem;

public partial class DisciplineManagementWindow : Window
{
    private List<Discipline> _disciplines;
    private Discipline? _selectedDiscipline;

    public DisciplineManagementWindow()
    {
        InitializeComponent();
        LoadData();
    }

    private async void LoadData()
    {
        try
        {
            using var db = new AppDbContext();
            _disciplines = await db.Disciplines.AsNoTracking().ToListAsync();
            RefreshDataGrid();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void RefreshDataGrid() => DisciplineDataGrid.ItemsSource = _disciplines;

    private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var q = SearchTextBox.Text.Trim().ToLower();
        DisciplineDataGrid.ItemsSource = string.IsNullOrEmpty(q)
            ? _disciplines
            : _disciplines.Where(d => d.Name.ToLower().Contains(q) || d.Code.ToLower().Contains(q)).ToList();
    }

    private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !Regex.IsMatch(e.Text, @"^\d+$");
    }

    private void AddDiscipline_Click(object sender, RoutedEventArgs e)
    {
        _selectedDiscipline = null;
        ClearForm();
        DisciplineNameTextBox.Focus();
    }

    private void EditDiscipline_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedDiscipline == null) { MessageBox.Show("Выберите дисциплину", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
        DisciplineNameTextBox.Text = _selectedDiscipline.Name;
        DisciplineCodeTextBox.Text = _selectedDiscipline.Code;
        LectureHoursTextBox.Text = _selectedDiscipline.LectureHours.ToString();
        SeminarHoursTextBox.Text = _selectedDiscipline.SeminarHours.ToString();
        LabHoursTextBox.Text = _selectedDiscipline.LabHours.ToString();
        CreditsTextBox.Text = _selectedDiscipline.Credits.ToString();
        DisciplineNameTextBox.Focus();
    }

    private async void DeleteDiscipline_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedDiscipline == null) { MessageBox.Show("Выберите дисциплину", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
        var result = MessageBox.Show($"Удалить дисциплину '{_selectedDiscipline.Name}'?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result == MessageBoxResult.Yes)
        {
            try
            {
                using var db = new AppDbContext();
                var d = await db.Disciplines.FindAsync(_selectedDiscipline.Id);
                if (d != null) { db.Disciplines.Remove(d); await db.SaveChangesAsync(); }
                LoadData();
                ClearForm();
            }
            catch (Exception ex) { MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); }
        }
    }

    private async void SaveDiscipline_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(DisciplineNameTextBox.Text)) { MessageBox.Show("Введите название", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
        if (string.IsNullOrWhiteSpace(DisciplineCodeTextBox.Text)) { MessageBox.Show("Введите код", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
        if (!int.TryParse(CreditsTextBox.Text, out int credits) || credits <= 0) { MessageBox.Show("Введите корректные кредиты", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning); return; }

        try
        {
            using var db = new AppDbContext();
            int lec = int.TryParse(LectureHoursTextBox.Text, out int l) ? l : 0;
            int sem = int.TryParse(SeminarHoursTextBox.Text, out int s) ? s : 0;
            int lab = int.TryParse(LabHoursTextBox.Text, out int lb) ? lb : 0;

            if (_selectedDiscipline == null)
            {
                if (await db.Disciplines.AnyAsync(d => d.Code == DisciplineCodeTextBox.Text.Trim())) { MessageBox.Show("Дисциплина с таким кодом уже существует", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
                db.Disciplines.Add(new Discipline { Name = DisciplineNameTextBox.Text.Trim(), Code = DisciplineCodeTextBox.Text.Trim().ToUpper(), LectureHours = lec, SeminarHours = sem, LabHours = lab, Credits = credits });
            }
            else
            {
                if (await db.Disciplines.AnyAsync(d => d.Code == DisciplineCodeTextBox.Text.Trim() && d.Id != _selectedDiscipline.Id)) { MessageBox.Show("Дисциплина с таким кодом уже существует", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
                var d = await db.Disciplines.FindAsync(_selectedDiscipline.Id);
                if (d != null) { d.Name = DisciplineNameTextBox.Text.Trim(); d.Code = DisciplineCodeTextBox.Text.Trim().ToUpper(); d.LectureHours = lec; d.SeminarHours = sem; d.LabHours = lab; d.Credits = credits; db.Disciplines.Update(d); }
            }
            await db.SaveChangesAsync();
            LoadData();
            ClearForm();
            MessageBox.Show("Дисциплина сохранена", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex) { MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); }
    }

    private void CancelDiscipline_Click(object sender, RoutedEventArgs e) => ClearForm();

    private void ClearForm()
    {
        _selectedDiscipline = null;
        DisciplineNameTextBox.Text = DisciplineCodeTextBox.Text = LectureHoursTextBox.Text = SeminarHoursTextBox.Text = LabHoursTextBox.Text = CreditsTextBox.Text = "";
    }

    private void DisciplineDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DisciplineDataGrid.SelectedItem is Discipline d) _selectedDiscipline = d;
    }
}
