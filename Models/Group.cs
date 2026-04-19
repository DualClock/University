namespace UniversitySystem.Models;

public class Group
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Year { get; set; }
    public int DepartmentId { get; set; }
    public Department? Department { get; set; }
    public ICollection<User> Students { get; set; } = new List<User>();
    public ICollection<Curriculum> Curricula { get; set; } = new List<Curriculum>();
}