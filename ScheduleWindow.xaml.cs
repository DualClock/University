using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using UniversitySystem.Data;
using UniversitySystem.Models;

namespace UniversitySystem;

public partial class ScheduleWindow : Window
{
    private List<Schedule> _schedules = new();
    private List<Group> _groups = new();
    private List<Discipline> _disciplines = new();
    private Schedule? _selected;

    public ScheduleWindow()
    {
        InitializeComponent();
        LoadData();
    }

    private async void LoadData()
    {
        using var db = new AppDbContext();
        _groups = await db.Groups.AsNoTracking().ToListAsync();
        _disciplines = await db.Disciplines.AsNoTracking().ToListAsync();
        _schedules = await db.Schedules.Include(s => s.Group).Include(s => s.Discipline).AsNoTracking().ToListAsync();
        GroupFilterComboBox.ItemsSource = _groups;
        GroupComboBox.ItemsSource = _groups;
        DisciplineComboBox.ItemsSource = _disciplines;
        RefreshDataGrid();
    }

    private void RefreshDataGrid() => ScheduleDataGrid.ItemsSource = _schedules;

    private void ApplyFilters()
    {
        var groupId = GroupFilterComboBox.SelectedValue as int?;
        var dayTag = (DayFilterComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString();
        int day = int.TryParse(dayTag, out int d) ? d : 0;
        var result = _schedules.AsEnumerable();
        if (groupId > 0) result = result.Where(s => s.GroupId == groupId);
        if (day > 0) result = result.Where(s => s.DayOfWeek == day);
        ScheduleDataGrid.ItemsSource = result.ToList();
    }

    private void Filter_Changed(object s, SelectionChangedEventArgs e) => ApplyFilters();

    private void AddSchedule_Click(object s, RoutedEventArgs e) { _selected = null; ClearForm(); }

    private async void DeleteSchedule_Click(object s, RoutedEventArgs e)
    {
        if (_selected == null) { MessageBox.Show("Выберите запись"); return; }
        var r = MessageBox.Show("Удалить?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (r == MessageBoxResult.Yes)
        {
            using var db = new AppDbContext();
            var sch = await db.Schedules.FindAsync(_selected.Id);
            if (sch != null) { db.Schedules.Remove(sch); await db.SaveChangesAsync(); }
            LoadData();
        }
    }

    private async void SaveSchedule_Click(object s, RoutedEventArgs e)
    {
        if (GroupComboBox.SelectedValue == null || DisciplineComboBox.SelectedValue == null) { MessageBox.Show("Заполните обязательные поля"); return; }
        if (!TimeOnly.TryParse(StartTimeTextBox.Text, out var start)) { MessageBox.Show("Неверный формат времени начала"); return; }
        if (!TimeOnly.TryParse(EndTimeTextBox.Text, out var end)) { MessageBox.Show("Неверный формат времени конца"); return; }
        try
        {
            using var db = new AppDbContext();
            int day = int.Parse(((ComboBoxItem)DayComboBox.SelectedItem).Tag.ToString()!);
            string type = ((ComboBoxItem)TypeComboBox.SelectedItem).Tag.ToString()!;
            if (_selected == null)
            {
                db.Schedules.Add(new Schedule { GroupId = (int)GroupComboBox.SelectedValue, DisciplineId = (int)DisciplineComboBox.SelectedValue, DayOfWeek = day, StartTime = start, EndTime = end, Room = RoomTextBox.Text.Trim(), Type = type, TeacherId = 1 });
            }
            else
            {
                var sch = await db.Schedules.FindAsync(_selected.Id);
                if (sch != null) { sch.GroupId = (int)GroupComboBox.SelectedValue; sch.DisciplineId = (int)DisciplineComboBox.SelectedValue; sch.DayOfWeek = day; sch.StartTime = start; sch.EndTime = end; sch.Room = RoomTextBox.Text.Trim(); sch.Type = type; db.Update(sch); }
            }
            await db.SaveChangesAsync();
            LoadData();
            ClearForm();
            MessageBox.Show("Сохранено", "Успех");
        }
        catch (Exception ex) { MessageBox.Show($"Ошибка: {ex.Message}"); }
    }

    private void CancelSchedule_Click(object s, RoutedEventArgs e) => ClearForm();
    private void ClearForm() { _selected = null; RoomTextBox.Text = ""; StartTimeTextBox.Text = "09:00"; EndTimeTextBox.Text = "10:30"; }
    private void ScheduleDataGrid_SelectionChanged(object s, SelectionChangedEventArgs e) { if (ScheduleDataGrid.SelectedItem is Schedule sch) _selected = sch; }
}
