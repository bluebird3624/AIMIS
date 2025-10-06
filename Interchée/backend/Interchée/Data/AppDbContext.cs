using Interchée.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Interchée.Data
{
    public class AppDbContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Department> Departments => Set<Department>();
        public DbSet<DepartmentRoleAssignment> DepartmentRoleAssignments => Set<DepartmentRoleAssignment>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        //Absence Management

        public DbSet<AbsenceRequest> AbsenceRequests => Set<AbsenceRequest>();
        public DbSet<AbsenceDecision> AbsenceDecisions => Set<AbsenceDecision>();
        public DbSet<AbsenceLimitPolicy> AbsenceLimitPolicies => Set<AbsenceLimitPolicy>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);

            // AppUser name fields
            b.Entity<AppUser>(e =>
            {
                e.Property(x => x.FirstName).HasMaxLength(64).IsRequired();
                e.Property(x => x.LastName).HasMaxLength(64).IsRequired();
                e.Property(x => x.MiddleName).HasMaxLength(64);
            });

            // Department
            b.Entity<Department>(e =>
            {
                e.Property(x => x.Name).HasMaxLength(128).IsRequired();
                e.HasIndex(x => x.Name).IsUnique();
                e.Property(x => x.Code).HasMaxLength(32);
            });

            // DepartmentRoleAssignment (User ↔ RoleName ↔ Department)
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

            // RefreshToken (optional but mapped)
            b.Entity<RefreshToken>(e =>
            {
                e.HasIndex(x => x.Token).IsUnique();
                e.Property(x => x.Token).HasMaxLength(512).IsRequired();

                e.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            
            // AbsenceRequest
            b.Entity<AbsenceRequest>(e =>
            {
                e.Property(x => x.Reason).IsRequired();
                e.Property(x => x.Status).HasMaxLength(32).IsRequired();
                e.Property(x => x.Days).HasPrecision(4, 1);

                e.HasIndex(x => x.UserId);
                e.HasIndex(x => x.DepartmentId);
                e.HasIndex(x => new { x.Status, x.DepartmentId });

                e.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.Department)
                    .WithMany()
                    .HasForeignKey(x => x.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // AbsenceDecision
            b.Entity<AbsenceDecision>(e =>
            {
                e.Property(x => x.Decision).HasMaxLength(32).IsRequired();
                e.Property(x => x.Comment);

                e.HasOne(x => x.Request)
                    .WithOne(x => x.Decision)
                    .HasForeignKey<AbsenceDecision>(x => x.RequestId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.DecidedByUser)
                    .WithMany()
                    .HasForeignKey(x => x.DecidedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // AbsenceLimitPolicy
            b.Entity<AbsenceLimitPolicy>(e =>
            {
                e.Property(x => x.Scope).HasMaxLength(32).IsRequired();
                e.Property(x => x.MaxDaysPerTerm).HasPrecision(5, 2);
                e.Property(x => x.MaxDaysPerMonth).HasPrecision(4, 1);

                e.HasIndex(x => new { x.Scope, x.DepartmentId, x.EffectiveFrom });

                e.HasOne(x => x.Department)
                    .WithMany()
                    .HasForeignKey(x => x.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
