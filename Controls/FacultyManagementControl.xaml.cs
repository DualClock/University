using System; using System.Collections.Generic; using System.Linq; using System.Windows; using System.Windows.Controls; using Microsoft.EntityFrameworkCore; using UniversitySystem.Data; using UniversitySystem.Models;
namespace UniversitySystem.Controls{
public partial class FacultyManagementControl : UserControl{
    private List<Faculty> _faculties = new(); private Faculty? _selected;
    public FacultyManagementControl(){ InitializeComponent(); _ = LoadFaculties(); }
    private async System.Threading.Tasks.Task LoadFaculties(){ using var db = new AppDbContext(); _faculties = await db.Faculties.ToListAsync(); FacultyDataGrid.ItemsSource = _faculties; }
    private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e){ var q = SearchTextBox.Text.Trim().ToLower(); FacultyDataGrid.ItemsSource = string.IsNullOrEmpty(q) ? _faculties : _faculties.Where(f => f.Name.ToLower().Contains(q)).ToList(); }
    private void AddFaculty_Click(object sender, RoutedEventArgs e){ _selected = null; FacultyNameTextBox.Text = ""; }
    private void FacultyDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e){ if (FacultyDataGrid.SelectedItem is Faculty f){ _selected = f; FacultyNameTextBox.Text = f.Name; } }
    private async void SaveFaculty_Click(object sender, RoutedEventArgs e){ if (string.IsNullOrWhiteSpace(FacultyNameTextBox.Text)){ MessageBox.Show("Введите название"); return; } try{ using var db = new AppDbContext(); if (_selected == null){ db.Faculties.Add(new Faculty { Name = FacultyNameTextBox.Text.Trim() }); } else { var f = await db.Faculties.FindAsync(_selected.Id); if (f != null){ f.Name = FacultyNameTextBox.Text.Trim(); db.Update(f); } } await db.SaveChangesAsync(); await LoadFaculties(); FacultyNameTextBox.Text = ""; _selected = null; MessageBox.Show("Сохранено"); } catch (Exception ex){ MessageBox.Show($"Ошибка: {ex.Message}"); } }
    private void ClearForm_Click(object sender, RoutedEventArgs e){ _selected = null; FacultyNameTextBox.Text = ""; }}}
