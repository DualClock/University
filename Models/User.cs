using UniversitySystem.Models;

public class User
{
    public int Id { get; set; }
    public string Login { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty; // БЫЛО: PasswordHash
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Role { get; set; } = "Student";
    public int? GroupId { get; set; }
    public int? DepartmentId { get; set; }

    public Group? Group { get; set; }
    public Department? Department { get; set; }
    public ICollection<Grade> Grades { get; set; } = new List<Grade>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}