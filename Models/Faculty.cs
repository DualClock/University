using DocumentFormat.OpenXml.Bibliography;

namespace UniversitySystem.Models;

public class Faculty
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public ICollection<Department> Departments { get; set; } = new List<Department>();
}   