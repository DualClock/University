namespace UniversitySystem.Models;

public class RatingItem
{
    public int Rank { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public decimal AverageGrade { get; set; }
}