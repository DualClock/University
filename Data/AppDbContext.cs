using Microsoft.EntityFrameworkCore;
using UniversitySystem.Models;

namespace UniversitySystem.Data;

public class AppDbContext : DbContext
{
    public AppDbContext() { }
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Faculty> Faculties => Set<Faculty>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<Discipline> Disciplines => Set<Discipline>();
    public DbSet<Curriculum> Curricula => Set<Curriculum>();
    public DbSet<Schedule> Schedules => Set<Schedule>();
    public DbSet<Grade> Grades => Set<Grade>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<UserGroup> UserGroups => Set<UserGroup>();
    public DbSet<UserDepartment> UserDepartments => Set<UserDepartment>();
    public DbSet<TeacherDisciplineGroup> TeacherDisciplineGroups => Set<TeacherDisciplineGroup>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {   
            optionsBuilder.UseSqlServer(
                @"Data Source=(localdb)\MSSQLLocalDB;Database=UniversityDB;Integrated Security=True;TrustServerCertificate=True;Encrypt=True;Command Timeout=30");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasIndex(u => u.Login).IsUnique();
        modelBuilder.Entity<Faculty>().HasIndex(f => f.Code).IsUnique();
        modelBuilder.Entity<Department>().HasIndex(d => d.Code).IsUnique();
        modelBuilder.Entity<Group>().HasIndex(g => g.Name).IsUnique();
        modelBuilder.Entity<Discipline>().HasIndex(d => d.Code).IsUnique();
        modelBuilder.Entity<Curriculum>().HasIndex(c => new { c.GroupId, c.DisciplineId, c.Semester, c.AcademicYear }).IsUnique();
        modelBuilder.Entity<Grade>().HasIndex(g => new { g.StudentId, g.DisciplineId });
        modelBuilder.Entity<Notification>().HasIndex(n => new { n.UserId, n.IsRead });

        modelBuilder.Entity<Department>()
            .HasOne(d => d.Faculty).WithMany(f => f.Departments).HasForeignKey(d => d.FacultyId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Group>()
            .HasOne(g => g.Department).WithMany(d => d.Groups).HasForeignKey(g => g.DepartmentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasOne(u => u.Group).WithMany(g => g.Students).HasForeignKey(u => u.GroupId);

        modelBuilder.Entity<User>()
            .HasOne(u => u.Department).WithMany(d => d.Teachers).HasForeignKey(u => u.DepartmentId);

        modelBuilder.Entity<Schedule>()
            .HasOne(s => s.Teacher).WithMany().HasForeignKey(s => s.TeacherId);

        modelBuilder.Entity<UserGroup>()
            .HasKey(ug => new { ug.UserId, ug.GroupId });
        modelBuilder.Entity<UserDepartment>()
            .HasKey(ud => new { ud.UserId, ud.DepartmentId });
    }
}