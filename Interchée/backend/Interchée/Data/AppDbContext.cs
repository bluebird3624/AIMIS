using Interchée.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Interchée.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>(options)
    {

        // Existing DbSets
        public DbSet<Department> Departments => Set<Department>();
        public DbSet<DepartmentRoleAssignment> DepartmentRoleAssignments => Set<DepartmentRoleAssignment>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        // Absence Management Module DbSets
        public DbSet<Intern> Interns => Set<Intern>();
        public DbSet<AbsenceRequest> AbsenceRequests => Set<AbsenceRequest>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);

            ConfigureAppUser(b);
            ConfigureDepartment(b);
            ConfigureDepartmentRoleAssignment(b);
            ConfigureRefreshToken(b);
            ConfigureIntern(b);
            ConfigureAbsenceRequest(b);
        }

        private static void ConfigureAppUser(ModelBuilder b)
        {
            b.Entity<AppUser>(e =>
            {
                e.Property(x => x.FirstName).HasMaxLength(64).IsRequired();
                e.Property(x => x.LastName).HasMaxLength(64).IsRequired();
                e.Property(x => x.MiddleName).HasMaxLength(64);

                // Additional properties for absence management
                e.Property(x => x.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()");

                e.Property(x => x.IsActive)
                    .IsRequired()
                    .HasDefaultValue(true);

                // Navigation properties configuration
                e.HasMany(u => u.SupervisedInterns)
                    .WithOne(i => i.Supervisor)
                    .HasForeignKey(i => i.SupervisorId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasMany(u => u.ApprovedAbsenceRequests)
                    .WithOne(ar => ar.ApprovedBy)
                    .HasForeignKey(ar => ar.ApprovedById)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private static void ConfigureDepartment(ModelBuilder b)
        {
            b.Entity<Department>(e =>
            {
                e.Property(x => x.DepartmentName).HasMaxLength(128).IsRequired();
                e.HasIndex(x => x.DepartmentName).IsUnique();
                e.Property(x => x.Code).HasMaxLength(32);
               
            });
        }

        private static void ConfigureDepartmentRoleAssignment(ModelBuilder b)
        {
            b.Entity<DepartmentRoleAssignment>(e =>
            {
                e.Property(x => x.RoleName).HasMaxLength(64).IsRequired();
                e.HasIndex(x => new { x.UserId, x.DepartmentId, x.RoleName }).IsUnique();

                e.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.Department)
                    .WithMany(d => d.Assignments)
                    .HasForeignKey(x => x.DepartmentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private static void ConfigureRefreshToken(ModelBuilder b)
        {
            b.Entity<RefreshToken>(e =>
            {
                e.HasIndex(x => x.Token).IsUnique();
                e.Property(x => x.Token).HasMaxLength(512).IsRequired();

                e.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private static void ConfigureIntern(ModelBuilder b)
        {
            b.Entity<Intern>(e =>
            {
                e.HasKey(i => i.Id);

                // Properties
                e.Property(i => i.StartDate)
                    .IsRequired();

                e.Property(i => i.EndDate)
                    .IsRequired();

                e.Property(i => i.University)
                    .HasMaxLength(200);

                e.Property(i => i.CourseOfStudy)
                    .HasMaxLength(200);

                e.Property(i => i.Status)
                    .IsRequired()
                    .HasConversion<int>()
                    .HasDefaultValue(InternStatus.Active);

                // Relationships
                e.HasOne(i => i.User)
                    .WithOne()
                    .HasForeignKey<Intern>(i => i.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Supervisor relationship is now configured in AppUser
                // Intern -> AbsenceRequests relationship
                e.HasMany(i => i.AbsenceRequests)
                    .WithOne(ar => ar.Intern)
                    .HasForeignKey(ar => ar.InternId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Indexes
                e.HasIndex(i => i.UserId)
                    .IsUnique();

                e.HasIndex(i => i.SupervisorId);
                e.HasIndex(i => i.Status);
                e.HasIndex(i => i.StartDate);
                e.HasIndex(i => i.EndDate);
                e.HasIndex(i => new { i.StartDate, i.EndDate });
            });
        }

        private static void ConfigureAbsenceRequest(ModelBuilder b)
        {
            b.Entity<AbsenceRequest>(e =>
            {
                e.HasKey(ar => ar.Id);

                // Properties
                e.Property(ar => ar.Reason)
                    .IsRequired()
                    .HasMaxLength(500);

                e.Property(ar => ar.StartDate)
                    .IsRequired();

                e.Property(ar => ar.EndDate)
                    .IsRequired();

                e.Property(ar => ar.Status)
                    .IsRequired()
                    .HasConversion<int>()
                    .HasDefaultValue(AbsenceStatus.Pending);

                e.Property(ar => ar.RejectionReason)
                    .HasMaxLength(500);

                e.Property(ar => ar.RequestedAt)
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()");

                // Relationships are now configured in AppUser and Intern
                // Only configure foreign keys and indexes here

                // Indexes for performance
                e.HasIndex(ar => ar.InternId);
                e.HasIndex(ar => ar.ApprovedById);
                e.HasIndex(ar => ar.Status);
                e.HasIndex(ar => ar.StartDate);
                e.HasIndex(ar => ar.EndDate);
                e.HasIndex(ar => ar.RequestedAt);
                e.HasIndex(ar => new { ar.InternId, ar.Status });
                e.HasIndex(ar => new { ar.StartDate, ar.EndDate });
                e.HasIndex(ar => new { ar.Status, ar.RequestedAt });
            });
        }
    }
}