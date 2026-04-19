namespace UniversitySystem.Models;

public class Department
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int FacultyId { get; set; }
    public Faculty? Faculty { get; set; }
    public ICollection<Group> Groups { get; set; } = new List<Group>();
    public ICollection<User> Teachers { get; set; } = new List<User>();
}