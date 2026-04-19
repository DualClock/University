namespace UniversitySystem.Models;

public class Discipline
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int LectureHours { get; set; }
    public int SeminarHours { get; set; }
    public int LabHours { get; set; }
    public int Credits { get; set; }
    public ICollection<Grade> Grades { get; set; } = new List<Grade>();
}