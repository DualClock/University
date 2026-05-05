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
using UniversitySystem.Services;

namespace UniversitySystem;

public partial class GradeManagementWindow : Window
{
    private List<Grade> _grades = new();
    private List<User> _students = new();
    private List<User> _allStudents = new();

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
        
        _allStudents = await db.Users.Where(u => u.Role == "Student").AsNoTracking().ToListAsync();
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
            var semesterStr = (SemesterComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
            int semester = int.TryParse(semesterStr, out int s) ? s : 1;
            string year = YearTextBox.Text.Trim();
            string type = ExamRadio.IsChecked == true ? "Exam" : SeminarRadio.IsChecked == true ? "Seminar" : "Lab";
            
            _students = await db.Users.Where(u => u.GroupId == groupId && u.Role == "Student").ToListAsync();
            
            if (_students.Count == 0)
            {
                MessageBox.Show("В группе нет студентов", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            
            var studentIds = _students.Select(s => s.Id).ToList();
            var existingGrades = await db.Grades
                .Where(g => g.DisciplineId == disciplineId && studentIds.Contains(g.StudentId) && g.Semester == semester && g.AcademicYear == year && g.Type == type)
                .Include(g => g.Student)
                .ToListAsync();
            
            _grades = existingGrades;
            GradesDataGrid.ItemsSource = null;
            GradesDataGrid.ItemsSource = _students;
        }
        catch (Exception ex) { MessageBox.Show($"Ошибка: {ex.Message}"); }
    }

    private void GradeInput_PreviewTextInput(object s, TextCompositionEventArgs e) { e.Handled = !Regex.IsMatch(e.Text, @"^[2-5]"); }
    private void GradeInput_TextChanged(object s, TextChangedEventArgs e) { }
    private void GradeType_Changed(object s, RoutedEventArgs e) { }
    private void GroupComboBox_SelectionChanged(object s, SelectionChangedEventArgs e) { }

    private async void AddGrade_Click(object sender, RoutedEventArgs e)
    {
        if (GroupComboBox.SelectedValue == null) { MessageBox.Show("Выберите группу"); return; }
        if (DisciplineComboBox.SelectedValue == null) { MessageBox.Show("Выберите дисциплину"); return; }
        
        try
        {
            int groupId = (int)GroupComboBox.SelectedValue;
            int disciplineId = (int)DisciplineComboBox.SelectedValue;
            var semesterStr = (SemesterComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
            int semester = int.TryParse(semesterStr, out int s) ? s : 1;
            string year = YearTextBox.Text.Trim();
            string type = ExamRadio.IsChecked == true ? "Exam" : SeminarRadio.IsChecked == true ? "Seminar" : "Lab";
            
            using var db = new AppDbContext();
            var discipline = await db.Disciplines.FindAsync(disciplineId);
            var group = await db.Groups.FindAsync(groupId);
            
            var dialog = new Window
            {
                Title = "Добавить оценку",
                Width = 450,
                Height = 400,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.NoResize,
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(248, 249, 250))
            };
            
            var grid = new Grid { Margin = new Thickness(20) };
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            
            var titleText = new TextBlock
            {
                Text = $"Добавить оценку по {discipline?.Name}",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 20)
            };
            Grid.SetRow(titleText, 0);
            grid.Children.Add(titleText);
            
            var studentLabel = new TextBlock { Text = "Студент:", Margin = new Thickness(0, 0, 0, 5) };
            Grid.SetRow(studentLabel, 1);
            grid.Children.Add(studentLabel);
            
            var studentsInGroup = _allStudents.Where(u => u.GroupId == groupId && u.Role == "Student").ToList();
            var studentCombo = new ComboBox { Margin = new Thickness(0, 0, 0, 15), DisplayMemberPath = "FullName", SelectedValuePath = "Id" };
            studentCombo.ItemsSource = studentsInGroup;
            Grid.SetRow(studentCombo, 2);
            grid.Children.Add(studentCombo);
            
            var typeLabel = new TextBlock { Text = "Тип оценки:", Margin = new Thickness(0, 0, 0, 5) };
            Grid.SetRow(typeLabel, 3);
            grid.Children.Add(typeLabel);
            
            var typeCombo = new ComboBox { Margin = new Thickness(0, 0, 0, 15) };
            typeCombo.Items.Add(new ComboBoxItem { Content = "Seminar", IsSelected = type == "Seminar" });
            typeCombo.Items.Add(new ComboBoxItem { Content = "Lab", IsSelected = type == "Lab" });
            typeCombo.Items.Add(new ComboBoxItem { Content = "Exam", IsSelected = type == "Exam" });
            Grid.SetRow(typeCombo, 4);
            grid.Children.Add(typeCombo);
            
            var gradeLabel = new TextBlock { Text = "Оценка (2-5):", Margin = new Thickness(0, 0, 0, 5) };
            Grid.SetRow(gradeLabel, 5);
            grid.Children.Add(gradeLabel);
            
            var gradeInput = new TextBox { Margin = new Thickness(0, 0, 0, 15) };
            Grid.SetRow(gradeInput, 6);
            grid.Children.Add(gradeInput);
            
            var saveButton = new Button
            {
                Content = "Сохранить",
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(40, 167, 69)),
                Foreground = System.Windows.Media.Brushes.White,
                Padding = new Thickness(20, 10, 20, 10),
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 10, 0, 0)
            };
            
            saveButton.Click += async (s, args) =>
            {
                if (studentCombo.SelectedValue == null)
                {
                    MessageBox.Show("Выберите студента", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                var selectedType = ((ComboBoxItem)typeCombo.SelectedItem)?.Content?.ToString() ?? "Seminar";
                
                if (!decimal.TryParse(gradeInput.Text, out decimal value) || value < 2 || value > 5)
                {
                    MessageBox.Show("Введите оценку от 2 до 5", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                try
                {
                    int studentId = (int)studentCombo.SelectedValue;
                    var existing = await db.Grades.FirstOrDefaultAsync(g => 
                        g.StudentId == studentId && 
                        g.DisciplineId == disciplineId && 
                        g.Type == selectedType && 
                        g.Semester == semester && 
                        g.AcademicYear == year);
                    
                    if (existing != null)
                    {
                        existing.Value = value;
                        existing.Date = DateTime.UtcNow;
                        db.Update(existing);
                    }
                    else
                    {
                        db.Grades.Add(new Grade
                        {
                            StudentId = studentId,
                            DisciplineId = disciplineId,
                            Type = selectedType,
                            Value = value,
                            Semester = semester,
                            AcademicYear = year,
                            Date = DateTime.UtcNow
                        });
                    }
                    
                    var student = await db.Users.FindAsync(studentId);
                    if (student != null && discipline != null)
                    {
                        string gradeText = value switch
                        {
                            >= 5 => "отлично",
                            >= 4 => "хорошо",
                            >= 3 => "удовлетворительно",
                            _ => "неудовлетворительно"
                        };
                        
                        await NotificationService.CreateAsync(
                            studentId,
                            $"Оценка по {discipline.Name}",
                            $"Вам выставлена оценка {value} ({gradeText}) по дисциплине '{discipline.Name}'",
                            "Grade"
                        );
                    }
                    
                    await db.SaveChangesAsync();
                    MessageBox.Show("Оценка добавлена", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    dialog.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };
            
            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal };
            buttonPanel.Children.Add(saveButton);
            Grid.SetRow(buttonPanel, 6);
            grid.Children.Add(buttonPanel);
            
            dialog.Content = grid;
            dialog.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

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
