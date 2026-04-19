namespace UniversitySystem.Models;

public class Curriculum
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public int DisciplineId { get; set; }
    public int Semester { get; set; }
    public string AcademicYear { get; set; } = string.Empty;
    public Group? Group { get; set; }
    public Discipline? Discipline { get; set; }
}