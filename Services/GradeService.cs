using Microsoft.EntityFrameworkCore;
using UniversitySystem.Data;
using UniversitySystem.Models;

namespace UniversitySystem.Services;

public static class GradeService
{
    public static async Task<List<Grade>> GetByGroupAndDisciplineAsync(int groupId, int disciplineId)
    {
        await using var db = new AppDbContext();
        return await db.Grades
            .AsNoTracking()
            .Include(g => g.Student)
            .Include(g => g.Discipline)
            .Where(g => g.Student!.GroupId == groupId && g.DisciplineId == disciplineId)
            .OrderBy(g => g.Student!.FullName)
            .ToListAsync();
    }

    public static async Task<decimal> CalculateAverageAsync(int studentId, int disciplineId)
    {
        await using var db = new AppDbContext();
        var avg = await db.Grades
            .AsNoTracking()
            .Where(g => g.StudentId == studentId && g.DisciplineId == disciplineId && g.Type != "Exam")
            .AverageAsync(g => (decimal?)g.Value);
        return avg ?? 0m;
    }

    public static bool ValidateScale(decimal value) => value is >= 2 and <= 5; // или 0-100

    public static async Task<bool> SaveOrUpdateBulkAsync(List<Grade> grades)
    {
        if (grades.Any(g => !ValidateScale(g.Value)))
            throw new ArgumentException("Оценки должны быть в диапазоне 2-5");

        await using var db = new AppDbContext();
        await using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            foreach (var g in grades)
            {
                if (g.Id > 0)
                {
                    db.Attach(g);
                    db.Entry(g).State = EntityState.Modified;
                }
                else
                {
                    db.Grades.Add(g);
                }
            }

            await db.SaveChangesAsync();
            await transaction.CommitAsync();

            // Авто-уведомления о задолженностях
            foreach (var g in grades.Where(x => x.Value <= 2.5m))
                await NotificationService.CreateAsync(g.StudentId, "Низкая оценка",
                    $"Получено {g.Value} по дисциплине. Требуется отработка.", "Debt");

            return true;
        }
        catch { await transaction.RollbackAsync(); throw; }
    }
}