using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniversitySystem.Models;

[Table("TeacherDisciplineGroups")]
public class TeacherDisciplineGroup
{
    [Key]
    public int Id { get; set; }
    
    public int TeacherId { get; set; }
    
    public int DisciplineId { get; set; }
    
    public int GroupId { get; set; }
    
    public int Semester { get; set; } = 1;
    
    public string AcademicYear { get; set; } = "2024/2025";
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [ForeignKey("TeacherId")]
    public User? Teacher { get; set; }
    
    [ForeignKey("DisciplineId")]
    public Discipline? Discipline { get; set; }
    
    [ForeignKey("GroupId")]
    public Group? Group { get; set; }
}