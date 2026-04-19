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

public partial class GradeManagementWindow : Window
{
    private List<Grade> _grades = new();
    private List<User> _students = new();

    public GradeManagementWindow()
    {
        InitializeComponent();
        LoadFilters();
    }

    private async void LoadFilters()
    {
        using var db = new AppDbContext();
        var groups = await db.Groups.AsNoTracking().ToListAsync();
        var disciplines = await db.Disciplines.AsNoTracking().ToListAsync();
        GroupComboBox.ItemsSource = groups;
        DisciplineComboBox.ItemsSource = disciplines;
    }

    private async void LoadGrades_Click(object sender, RoutedEventArgs e)
    {
        if (GroupComboBox.SelectedValue == null) { MessageBox.Show("Выберите группу"); return; }
        if (DisciplineComboBox.SelectedValue == null) { MessageBox.Show("Выберите дисциплину"); return; }
        try
        {
            using var db = new AppDbContext();
            int groupId = (int)GroupComboBox.SelectedValue;
            int disciplineId = (int)DisciplineComboBox.SelectedValue;
            _students = await db.Users.Where(u => u.GroupId == groupId).ToListAsync();
            _grades = await db.Grades.Where(g => g.DisciplineId == disciplineId && _students.Select(s => s.Id).Contains(g.StudentId)).Include(g => g.Student).ToListAsync();
            GradesDataGrid.ItemsSource = null;
            GradesDataGrid.ItemsSource = _students;
        }
        catch (Exception ex) { MessageBox.Show($"Ошибка: {ex.Message}"); }
    }

    private void GradeInput_PreviewTextInput(object s, TextCompositionEventArgs e) { e.Handled = !Regex.IsMatch(e.Text, @"^[2-5]"); }
    private void GradeInput_TextChanged(object s, TextChangedEventArgs e) { }
    private void GradeType_Changed(object s, RoutedEventArgs e) { }
    private void GroupComboBox_SelectionChanged(object s, SelectionChangedEventArgs e) { }

    private async void SaveGrades_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            using var db = new AppDbContext();
            int disciplineId = (int)DisciplineComboBox.SelectedValue;
            var semesterStr = (SemesterComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
            int semester = int.TryParse(semesterStr, out int s) ? s : 1;
            string year = YearTextBox.Text.Trim();
            string type = ExamRadio.IsChecked == true ? "Exam" : SeminarRadio.IsChecked == true ? "Seminar" : "Lab";
            foreach (var st in _students)
            {
                var row = GradesDataGrid.Items.Cast<object>().Where(i => i is User u && u.Id == st.Id).Select((item, idx) => new { item, idx }).FirstOrDefault();
                if (row != null)
                {
                    var container = GradesDataGrid.ItemContainerGenerator.ContainerFromItem(row.item) as DataGridRow;
                    if (container != null)
                    {
                        var cell = GradesDataGrid.Columns[3].GetCellContent(container) as System.Windows.Controls.TextBox;
                        if (cell != null && decimal.TryParse(cell.Text, out decimal val) && val >= 2 && val <= 5)
                        {
                            var studentId = st.Id;
                            var existing = await db.Grades.FirstOrDefaultAsync(g => g.StudentId == studentId && g.DisciplineId == disciplineId && g.Type == type && g.Semester == semester && g.AcademicYear == year);
                            if (existing != null) { existing.Value = val; existing.Date = DateTime.UtcNow; db.Update(existing); }
                            else db.Grades.Add(new Grade { StudentId = st.Id, DisciplineId = disciplineId, Type = type, Value = val, Semester = semester, AcademicYear = year, Date = DateTime.UtcNow });
                        }
                    }
                }
            }
            await db.SaveChangesAsync();
            MessageBox.Show("Оценки сохранены", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex) { MessageBox.Show($"Ошибка: {ex.Message}"); }
    }
}
