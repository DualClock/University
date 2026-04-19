namespace UniversitySystem.Models;

public class Schedule
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public int DisciplineId { get; set; }
    public int TeacherId { get; set; }
    public int DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string Room { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Lecture, Seminar, Lab

    public Group? Group { get; set; }
    public Discipline? Discipline { get; set; }
    public User? Teacher { get; set; }
}