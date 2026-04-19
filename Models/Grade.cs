namespace UniversitySystem.Models;

public class Grade
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int DisciplineId { get; set; }
    public string Type { get; set; } = string.Empty; // Seminar, Lab, Exam
    public decimal Value { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public int Semester { get; set; }
    public string AcademicYear { get; set; } = string.Empty;

    public User? Student { get; set; }
    public Discipline? Discipline { get; set; }
}