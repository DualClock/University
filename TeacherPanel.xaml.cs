using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using UniversitySystem.Data;
using UniversitySystem.Models;
using UniversitySystem.Services;

namespace UniversitySystem;

public partial class TeacherPanel : UserControl
{
    private List<Group> _groups = new();
    private List<Discipline> _disciplines = new();
    private List<User> _students = new();

    public TeacherPanel()
    {
        InitializeComponent();
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        await LoadGroupsAsync();
        await LoadDisciplinesAsync();
        await LoadStudentsAsync();
        await LoadGradesAsync();
        await LoadExamFiltersAsync();
    }

    private async Task LoadGroupsAsync()
    {
        await using var db = new AppDbContext();
        _groups = await db.Groups.Include(g => g.Department).AsNoTracking().ToListAsync();
        DgGroups.ItemsSource = _groups;
    }

    private async Task LoadDisciplinesAsync()
    {
        await using var db = new AppDbContext();
        _disciplines = await db.Disciplines.AsNoTracking().ToListAsync();
        DgDisciplines.ItemsSource = _disciplines;

        CmbGradeDiscipline.ItemsSource = _disciplines;
        CmbGradeDiscipline.DisplayMemberPath = "Name";
        CmbGradeDiscipline.SelectedValuePath = "Id";
    }

    private async Task LoadStudentsAsync()
    {
        await using var db = new AppDbContext();
        _students = await db.Users.Where(u => u.Role == "Student").AsNoTracking().ToListAsync();
        CmbStudentForGrade.ItemsSource = _students;
        CmbStudentForGrade.DisplayMemberPath = "FullName";
        CmbStudentForGrade.SelectedValuePath = "Id";
    }

    private async Task LoadGradesAsync()
    {
        await using var db = new AppDbContext();
        var grades = await db.Grades
            .Include(g => g.Student)
            .Include(g => g.Discipline)
            .AsNoTracking()
            .ToListAsync();
        DgGrades.ItemsSource = grades;
    }

    private async Task LoadExamFiltersAsync()
    {
        CmbExamGroup.ItemsSource = _groups;
        CmbExamGroup.DisplayMemberPath = "Name";
        CmbExamGroup.SelectedValuePath = "Id";

        CmbExamDiscipline.ItemsSource = _disciplines;
        CmbExamDiscipline.DisplayMemberPath = "Name";
        CmbExamDiscipline.SelectedValuePath = "Id";

        CmbGradeGroup.ItemsSource = _groups;
        CmbGradeGroup.DisplayMemberPath = "Name";
        CmbGradeGroup.SelectedValuePath = "Id";

        CmbGradeDiscipline.ItemsSource = _disciplines;
        CmbGradeDiscipline.DisplayMemberPath = "Name";
        CmbGradeDiscipline.SelectedValuePath = "Id";

        CmbDebtorsGroup.ItemsSource = _groups;
        CmbDebtorsGroup.DisplayMemberPath = "Name";
        CmbDebtorsGroup.SelectedValuePath = "Id";

        CmbDebtorsDiscipline.ItemsSource = _disciplines;
        CmbDebtorsDiscipline.DisplayMemberPath = "Name";
        CmbDebtorsDiscipline.SelectedValuePath = "Id";
    }

    private async void DgGroups_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
    }

    private async void CmbDebtorsGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
    }

    private async void CmbGradeGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CmbGradeGroup.SelectedValue == null) return;

        int groupId = (int)CmbGradeGroup.SelectedValue;
        await using var db = new AppDbContext();
        var groupStudents = await db.Users
            .Where(u => u.GroupId == groupId && u.Role == "Student")
            .AsNoTracking()
            .ToListAsync();

        CmbStudentForGrade.ItemsSource = groupStudents;
        CmbStudentForGrade.DisplayMemberPath = "FullName";
        CmbStudentForGrade.SelectedValuePath = "Id";
    }

    private async void CmbGradeDiscipline_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CmbGradeGroup.SelectedValue == null || CmbGradeDiscipline.SelectedValue == null) return;

        int groupId = (int)CmbGradeGroup.SelectedValue;
        int disciplineId = (int)CmbGradeDiscipline.SelectedValue;

        await using var db = new AppDbContext();
        var groupStudentIds = await db.UserGroups
            .Where(ug => ug.GroupId == groupId)
            .Select(ug => ug.UserId)
            .ToListAsync();

        var studentsWithGrades = await db.Grades
            .Where(g => g.DisciplineId == disciplineId && groupStudentIds.Contains(g.StudentId))
            .Select(g => g.StudentId)
            .Distinct()
            .ToListAsync();

        var allGroupStudents = await db.Users
            .Where(u => u.GroupId == groupId && u.Role == "Student")
            .AsNoTracking()
            .ToListAsync();

        var studentsWithoutGrades = allGroupStudents.Where(s => !studentsWithGrades.Contains(s.Id)).ToList();

        CmbStudentForGrade.ItemsSource = studentsWithoutGrades;
        CmbStudentForGrade.DisplayMemberPath = "FullName";
        CmbStudentForGrade.SelectedValuePath = "Id";
    }

    private async void BtnAddGrade_Click(object sender, RoutedEventArgs e)
    {
        if (CmbStudentForGrade.SelectedValue == null || CmbGradeDiscipline.SelectedValue == null ||
            CmbGradeType.SelectedItem == null || string.IsNullOrWhiteSpace(TxtGradeValue.Text))
        {
            MessageBox.Show("Заполните все поля", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!decimal.TryParse(TxtGradeValue.Text, out decimal value) || value < 0 || value > 5)
        {
            MessageBox.Show("Оценка должна быть числом от 0 до 5", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            await using var db = new AppDbContext();
            var grade = new Grade
            {
                StudentId = (int)CmbStudentForGrade.SelectedValue,
                DisciplineId = (int)CmbGradeDiscipline.SelectedValue,
                Type = ((ComboBoxItem)CmbGradeType.SelectedItem).Content.ToString() ?? "Seminar",
                Value = value,
                Date = DateTime.UtcNow,
                Semester = 1,
                AcademicYear = "2024/2025"
            };
            db.Grades.Add(grade);
            await db.SaveChangesAsync();

            var student = await db.Users.FindAsync((int)CmbStudentForGrade.SelectedValue);
            var discipline = await db.Disciplines.FindAsync((int)CmbGradeDiscipline.SelectedValue);
            
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
                    student.Id,
                    $"Оценка по {discipline.Name}",
                    $"Вам выставлена оценка {value} ({gradeText}) по дисциплине '{discipline.Name}'",
                    "Grade"
                );
            }

            CmbStudentForGrade.SelectedIndex = -1;
            CmbGradeDiscipline.SelectedIndex = -1;
            CmbGradeType.SelectedIndex = -1;
            TxtGradeValue.Clear();
            AddGradePopup.IsOpen = false;

            await LoadGradesAsync();
            MessageBox.Show($"Оценка добавлена. Уведомление отправлено студенту.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BtnOpenAddGradeForm_Click(object sender, RoutedEventArgs e)
    {
        // Загружаем данные в комбобоксы перед открытием
        LoadGroupDisciplineFilters();
        AddGradePopup.IsOpen = true;
    }

    private async void LoadGroupDisciplineFilters()
    {
        await using var db = new AppDbContext();
        var groups = await db.Groups.AsNoTracking().ToListAsync();
        var disciplines = await db.Disciplines.AsNoTracking().ToListAsync();

        CmbGradeGroup.ItemsSource = groups;
        CmbGradeGroup.DisplayMemberPath = "Name";
        CmbGradeGroup.SelectedValuePath = "Id";

        CmbGradeDiscipline.ItemsSource = disciplines;
        CmbGradeDiscipline.DisplayMemberPath = "Name";
        CmbGradeDiscipline.SelectedValuePath = "Id";
    }

    private void BtnCancelGrade_Click(object sender, RoutedEventArgs e)
    {
        AddGradePopup.IsOpen = false;
        CmbGradeGroup.SelectedIndex = -1;
        CmbGradeDiscipline.SelectedIndex = -1;
        CmbStudentForGrade.SelectedIndex = -1;
        CmbGradeType.SelectedIndex = -1;
        TxtGradeValue.Clear();
    }

    private async void BtnExportGrades_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                FileName = $"Оценки_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            };

            if (saveDialog.ShowDialog() == true)
            {
                await using var db = new AppDbContext();
                var grades = await db.Grades
                    .Include(g => g.Student)
                    .Include(g => g.Discipline)
                    .ToListAsync();

                ExportService.ExportGradesToExcel(grades, saveDialog.FileName);
                MessageBox.Show($"Оценки экспортированы в {saveDialog.FileName}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void BtnExportStudents_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                FileName = $"Студенты_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            };

            if (saveDialog.ShowDialog() == true)
            {
                await using var db = new AppDbContext();
                var students = await db.Users.Where(u => u.Role == "Student").ToListAsync();

                ExportService.ExportStudentsToExcel(students, saveDialog.FileName);
                MessageBox.Show($"Студенты экспортированы в {saveDialog.FileName}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void BtnGenerateExamSheet_Click(object sender, RoutedEventArgs e)
    {
        if (CmbExamGroup.SelectedValue == null || CmbExamDiscipline.SelectedValue == null)
        {
            MessageBox.Show("Выберите группу и дисциплину", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                FileName = $"Ведомость_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            };

            if (saveDialog.ShowDialog() == true)
            {
                int groupId = (int)CmbExamGroup.SelectedValue;
                int disciplineId = (int)CmbExamDiscipline.SelectedValue;

                await using var db = new AppDbContext();
                var group = await db.Groups.FirstOrDefaultAsync(g => g.Id == groupId);
                var discipline = await db.Disciplines.FirstOrDefaultAsync(d => d.Id == disciplineId);

                if (group == null || discipline == null)
                {
                    MessageBox.Show("Группа или дисциплина не найдены", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var groupStudents = await db.Users
                    .Where(s => s.GroupId == groupId && s.Role == "Student")
                    .OrderBy(s => s.FullName)
                    .ToListAsync();

                if (groupStudents.Count == 0)
                {
                    MessageBox.Show("В группе нет студентов", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var grades = await db.Grades
                    .Where(g => g.DisciplineId == disciplineId)
                    .ToListAsync();

                ExportService.ExportExamSheet(groupId, disciplineId, discipline.Name, group.Name, groupStudents, grades, saveDialog.FileName);
                MessageBox.Show($"Ведомость экспортирована в {saveDialog.FileName}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void BtnFindDebtors_Click(object sender, RoutedEventArgs e)
    {
        if (CmbDebtorsGroup.SelectedValue == null || CmbDebtorsDiscipline.SelectedValue == null)
        {
            MessageBox.Show("Выберите группу и дисциплину", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            int groupId = (int)CmbDebtorsGroup.SelectedValue;
            int disciplineId = (int)CmbDebtorsDiscipline.SelectedValue;

            await using var db = new AppDbContext();
            var debtors = await db.Grades
                .Include(g => g.Student)
                .Include(g => g.Discipline)
                .Where(g => g.Student.GroupId == groupId && g.DisciplineId == disciplineId && g.Value < 3)
                .GroupBy(g => g.StudentId)
                .Select(g => new
                {
                    StudentName = g.FirstOrDefault()!.Student.FullName,
                    DisciplineName = g.FirstOrDefault()!.Discipline.Name,
                    LastGrade = g.OrderByDescending(x => x.Date).FirstOrDefault()!.Value,
                    Date = g.OrderByDescending(x => x.Date).FirstOrDefault()!.Date
                })
                .ToListAsync();

            DgDebtors.ItemsSource = debtors;

            if (debtors.Count == 0)
            {
                MessageBox.Show("Должники не найдены", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void BtnNotifyDebtors_Click(object sender, RoutedEventArgs e)
    {
        if (DgDebtors.ItemsSource == null || DgDebtors.Items.Count == 0)
        {
            MessageBox.Show("Сначала найдите должников", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            await using var db = new AppDbContext();
            var debtors = DgDebtors.ItemsSource as IEnumerable<dynamic>;
            int notificationsSent = 0;

            foreach (var debtor in debtors)
            {
                string studentName = debtor.StudentName;
                string disciplineName = debtor.DisciplineName;
                decimal grade = debtor.LastGrade;

                var student = await db.Users.FirstOrDefaultAsync(u => u.FullName == studentName && u.Role == "Student");
                if (student != null)
                {
                    await NotificationService.CreateAsync(
                        student.Id,
                        "Задолженность по предмету",
                        $"У вас задолженность по дисциплине '{disciplineName}'. Последняя оценка: {grade}. Обратитесь к преподавателю.",
                        "Debt"
                    );
                    notificationsSent++;
                }
            }

            MessageBox.Show($"Отправлено {notificationsSent} уведомлений", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка отправки уведомлений: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}