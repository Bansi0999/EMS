using Microsoft.EntityFrameworkCore;
using EMS.Models;

namespace EMS.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Designation> Designations { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Salary> Salaries { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<Level> Levels { get; set; }
        public DbSet<EmployeeLevel> EmployeeLevels { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Department)
                .WithMany(d => d.Employees)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Designation)
                .WithMany(d => d.Employees)
                .HasForeignKey(e => e.DesignationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Designation>()
                .HasOne(d => d.Department)
                .WithMany(dep => dep.Designations)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Employee>()
                .HasIndex(e => e.EmployeeCode)
                .IsUnique();

            modelBuilder.Entity<Employee>()
                .HasIndex(e => e.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasOne(u => u.Employee)
                .WithOne()
                .HasForeignKey<User>(u => u.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Salary>()
                .HasOne(s => s.Employee).WithMany().HasForeignKey(s => s.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LeaveRequest>()
                .HasOne(l => l.Employee).WithMany().HasForeignKey(l => l.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EmployeeLevel>()
                .HasOne(el => el.Employee).WithMany().HasForeignKey(el => el.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<EmployeeLevel>()
                .HasOne(el => el.Level).WithMany().HasForeignKey(el => el.LevelId)
                .OnDelete(DeleteBehavior.Restrict);


            // Seed Departments
            modelBuilder.Entity<Department>().HasData(
                new Department { DepartmentId = 1, DepartmentName = "Human Resources", Description = "Handles recruitment, employee relations and policies" },
                new Department { DepartmentId = 2, DepartmentName = "Information Technology", Description = "Manages software, hardware and IT infrastructure" },
                new Department { DepartmentId = 3, DepartmentName = "Finance", Description = "Handles accounting, budgeting and payroll" },
                new Department { DepartmentId = 4, DepartmentName = "Marketing", Description = "Handles branding, advertising and market research" }
            );

            // Seed Designations
            modelBuilder.Entity<Designation>().HasData(
                new Designation { DesignationId = 1, Title = "HR Manager", DepartmentId = 1 },
                new Designation { DesignationId = 2, Title = "HR Executive", DepartmentId = 1 },
                new Designation { DesignationId = 3, Title = "Software Engineer", DepartmentId = 2 },
                new Designation { DesignationId = 4, Title = "Senior Software Engineer", DepartmentId = 2 },
                new Designation { DesignationId = 5, Title = "System Administrator", DepartmentId = 2 },
                new Designation { DesignationId = 6, Title = "Accountant", DepartmentId = 3 },
                new Designation { DesignationId = 7, Title = "Finance Manager", DepartmentId = 3 },
                new Designation { DesignationId = 8, Title = "Marketing Executive", DepartmentId = 4 }
            );


            modelBuilder.Entity<Level>().HasData(
                new Level { LevelId = 1, LevelName = "Level 1 - Junior", Description = "Entry level" },
                new Level { LevelId = 2, LevelName = "Level 2 - Associate", Description = "Associate level" },
                new Level { LevelId = 3, LevelName = "Level 3 - Senior", Description = "Senior level" },
                new Level { LevelId = 4, LevelName = "Level 4 - Lead", Description = "Team lead level" },
                new Level { LevelId = 5, LevelName = "Level 5 - Manager", Description = "Management level" }
            );

            // Seed default admin user - password: Admin@123 (SHA256 hashed)
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserId = 1,
                    Username = "admin",
                    PasswordHash = "e86f78a8a3caf0b60d8e74e5942aa6d86dc150cd3c03338aef25b7d2d7e3acc7",
                    FullName = "System Administrator",
                    Role = "Admin"
                }
            );
        }
    }
}
