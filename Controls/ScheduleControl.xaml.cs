using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using UniversitySystem.Data;
using UniversitySystem.Models;

namespace UniversitySystem.Controls
{
    public partial class ScheduleControl : UserControl
    {
        private List<Schedule> _schedules = new();
        private List<Discipline> _disciplines = new();
        private List<Group> _groups = new();
        private Schedule? _selected;

        public ScheduleControl()
        {
            InitializeComponent();
            _ = LoadData();
        }

        private async System.Threading.Tasks.Task LoadData()
        {
            using var db = new AppDbContext();
            _disciplines = await db.Disciplines.ToListAsync();
            _groups = await db.Groups.ToListAsync();
            _schedules = await db.Schedules
                .Include(s => s.Discipline)
                .Include(s => s.Group)
                .ToListAsync();
            
            DisciplineComboBox.ItemsSource = _disciplines;
            GroupFilter.ItemsSource = _groups;
            GroupComboBox.ItemsSource = _groups;
            ScheduleDataGrid.ItemsSource = _schedules;
        }

        private void FilterChanged(object sender, object e)
        {
            var gid = (GroupFilter.SelectedItem as Group)?.Id;
            var dayItem = DayFilter.SelectedItem as ComboBoxItem;
            int? day = dayItem?.Tag != null ? int.TryParse(dayItem.Tag.ToString(), out int d) ? d : (int?)null : null;
            ScheduleDataGrid.ItemsSource = _schedules.Where(s => (gid == null || s.GroupId == gid) && (day == null || s.DayOfWeek == day)).ToList();
        }

        private void AddSchedule_Click(object sender, RoutedEventArgs e)
        {
            _selected = null;
            TimeTextBox.Text = "09:00";
            RoomTextBox.Text = "";
            DayComboBox.SelectedIndex = -1;
            DisciplineComboBox.SelectedIndex = -1;
            GroupComboBox.SelectedIndex = -1;
            ConflictWarning.Text = "";
        }

        private void ScheduleDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ScheduleDataGrid.SelectedItem is Schedule s)
            {
                _selected = s;
                TimeTextBox.Text = s.StartTime.ToString("HH:mm");
                RoomTextBox.Text = s.Room;
                DayComboBox.SelectedItem = DayComboBox.Items.Cast<ComboBoxItem>().FirstOrDefault(i => i.Tag?.ToString() == s.DayOfWeek.ToString());
                DisciplineComboBox.SelectedItem = _disciplines.FirstOrDefault(d => d.Id == s.DisciplineId);
                GroupComboBox.SelectedItem = _groups.FirstOrDefault(g => g.Id == s.GroupId);
                ConflictWarning.Text = "";
            }
        }

        /// <summary>
        /// Проверяет конфликты расписания: аудитория, группа, преподаватель
        /// </summary>
        private List<string> CheckConflicts(int? excludeId, int day, TimeOnly startTime, TimeOnly endTime, string room, int groupId, int teacherId)
        {
            var conflicts = new List<string>();

            foreach (var s in _schedules)
            {
                // Исключаем текущую запись при редактировании
                if (excludeId.HasValue && s.Id == excludeId.Value)
                    continue;

                // Проверяем тот же день
                if (s.DayOfWeek != day)
                    continue;

                // Проверяем пересечение времени
                if (startTime < s.EndTime && endTime > s.StartTime)
                {
                    // Конфликт по аудитории
                    if (!string.IsNullOrEmpty(room) && s.Room == room)
                    {
                        conflicts.Add($"⚠️ Аудитория {room} занята ({s.Discipline?.Name})");
                    }

                    // Конфликт по группе
                    if (s.GroupId == groupId)
                    {
                        conflicts.Add($"⚠️ Группа уже занята ({s.Discipline?.Name})");
                    }

                    // Конфликт по преподавателю
                    if (s.TeacherId == teacherId)
                    {
                        var db = new AppDbContext();
                        var teacher = db.Users.FirstOrDefault(u => u.Id == teacherId);
                        conflicts.Add($"⚠️ Преподаватель {teacher?.FullName ?? "занят"} ({s.Discipline?.Name})");
                    }
                }
            }

            return conflicts;
        }

        private async void SaveSchedule_Click(object sender, RoutedEventArgs e)
        {
            if (DayComboBox.SelectedItem == null || DisciplineComboBox.SelectedItem == null || GroupComboBox.SelectedItem == null)
            {
                MessageBox.Show("Заполните все обязательные поля (день, дисциплина, группа)", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(TimeTextBox.Text) || string.IsNullOrWhiteSpace(RoomTextBox.Text))
            {
                MessageBox.Show("Заполните время и аудиторию", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using var db = new AppDbContext();
                var dayItem = DayComboBox.SelectedItem as ComboBoxItem;
                int day = dayItem?.Tag != null ? int.Parse(dayItem.Tag.ToString()!) : 1;
                var did = (DisciplineComboBox.SelectedItem as Discipline)?.Id ?? 0;
                var gid = (GroupComboBox.SelectedItem as Group)?.Id ?? 0;
                var teacherId = UniversitySystem.RbacService.CurrentUserId;
                var room = RoomTextBox.Text.Trim();

                if (!TimeOnly.TryParse(TimeTextBox.Text.Trim(), out var time))
                {
                    MessageBox.Show("Неверный формат времени. Используйте формат ЧЧ:ММ (например, 09:00)", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var endTime = time.AddHours(2); // Занятие 2 часа

                // Проверка конфликтов
                var conflicts = CheckConflicts(_selected?.Id, day, time, endTime, room, gid, teacherId);

                if (conflicts.Count > 0)
                {
                    var result = MessageBox.Show(
                        $"Обнаружены конфликты:\n\n{string.Join("\n", conflicts)}\n\nПродолжить сохранение?",
                        "Предупреждение о конфликте",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result != MessageBoxResult.Yes)
                        return;
                }

                if (_selected == null)
                {
                    db.Schedules.Add(new Schedule
                    {
                        DayOfWeek = day,
                        StartTime = time,
                        EndTime = endTime,
                        DisciplineId = did,
                        GroupId = gid,
                        TeacherId = teacherId,
                        Room = room,
                        Type = "Lecture"
                    });
                }
                else
                {
                    var s = await db.Schedules.FindAsync(_selected.Id);
                    if (s != null)
                    {
                        s.DayOfWeek = day;
                        s.StartTime = time;
                        s.EndTime = endTime;
                        s.DisciplineId = did;
                        s.GroupId = gid;
                        s.TeacherId = teacherId;
                        s.Room = room;
                        db.Update(s);
                    }
                }

                await db.SaveChangesAsync();
                await LoadData();
                ClearForm();
                MessageBox.Show("Расписание сохранено", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelSchedule_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            _selected = null;
            TimeTextBox.Text = "09:00";
            RoomTextBox.Text = "";
            DayComboBox.SelectedIndex = -1;
            DisciplineComboBox.SelectedIndex = -1;
            GroupComboBox.SelectedIndex = -1;
            ConflictWarning.Text = "";
        }

        private async void DeleteSchedule_Click(object sender, RoutedEventArgs e)
        {
            if (_selected == null)
            {
                MessageBox.Show("Выберите занятие для удаления", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Удалить занятие?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                using var db = new AppDbContext();
                var schedule = await db.Schedules.FindAsync(_selected.Id);
                if (schedule != null)
                {
                    db.Schedules.Remove(schedule);
                    await db.SaveChangesAsync();
                    await LoadData();
                    ClearForm();
                    MessageBox.Show("Занятие удалено", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}