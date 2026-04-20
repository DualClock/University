using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using UniversitySystem.Data;
using UniversitySystem.Models;
using UniversitySystem.Services;

namespace UniversitySystem.Controls
{
    public partial class GradesViewControl : UserControl
    {
        private List<Grade> _grades;
        private List<Discipline> _disciplines;

        public GradesViewControl()
        {
            InitializeComponent();
            LoadData();
        }

        private async void LoadData()
        {
            using var db = new AppDbContext();
            _disciplines = await db.Disciplines.ToListAsync();
            DisciplineFilter.ItemsSource = _disciplines;

            if (RbacService.CurrentUserId > 0)
            {
                _grades = await db.Grades
                    .Include(g => g.Discipline)
                    .Where(g => g.StudentId == RbacService.CurrentUserId)
                    .ToListAsync();
                RefreshDataGrid();
            }
        }

        private void RefreshDataGrid()
        {
            var data = _grades
                .Select(g => new
                {
                    DisciplineName = g.Discipline != null ? g.Discipline.Name : string.Empty,
                    g.Type,
                    g.Value,
                    g.Date
                })
                .ToList();

            GradesDataGrid.ItemsSource = data;

            var avg = data.Count > 0 ? data.Average(g => (double)g.Value) : 0;
            AvgTextBox.Text = avg.ToString("F2");
        }

        private void FilterChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_grades == null) return;

            var did = (DisciplineFilter.SelectedItem as Discipline)?.Id;
            var sem = (SemesterFilter.SelectedItem as ComboBoxItem)?.Content?.ToString();

            var filtered = _grades
                .Where(g =>
                    (did == null || g.DisciplineId == did) &&
                    (sem == null || sem == "Все" || g.Semester.ToString() == sem))
                .Select(g => new
                {
                    DisciplineName = g.Discipline != null ? g.Discipline.Name : string.Empty,
                    g.Type,
                    g.Value,
                    g.Date
                })
                .ToList();

            GradesDataGrid.ItemsSource = filtered;

            var avg = filtered.Count > 0 ? filtered.Average(g => (double)g.Value) : 0;
            AvgTextBox.Text = avg.ToString("F2");
        }
    }
}
