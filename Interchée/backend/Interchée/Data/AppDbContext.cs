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

        public DbSet<OnboardingRequest> OnboardingRequests => Set<OnboardingRequest>();
        public DbSet<OnboardingDecision> OnboardingDecisions => Set<OnboardingDecision>();

        public DbSet<Department> Departments => Set<Department>();
        public DbSet<DepartmentRoleAssignment> DepartmentRoleAssignments => Set<DepartmentRoleAssignment>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

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

            b.Entity<OnboardingRequest>(e =>
            {
                e.Property(x => x.Email).HasMaxLength(256).IsRequired();

                e.Property(x => x.FirstName).HasMaxLength(64).IsRequired();
                e.Property(x => x.LastName).HasMaxLength(64).IsRequired();
                e.Property(x => x.MiddleName).HasMaxLength(64);

                e.Property(x => x.FullName).HasMaxLength(128).IsRequired();

                e.Property(x => x.Status).HasMaxLength(32).IsRequired(); // Pending/Approved/Rejected

                e.Property(x => x.RequestedAt).IsRequired();

                e.HasIndex(x => new { x.Email, x.Status })
                 .HasDatabaseName("IX_Onboard_Email_Status");

                e.HasOne(x => x.Department)
                 .WithMany()
                 .HasForeignKey(x => x.DepartmentId)
                 .OnDelete(DeleteBehavior.Restrict); // preserve history
            });

            // OnboardingDecision
            b.Entity<OnboardingDecision>(e =>
            {
                e.ToTable("OnboardingDecisions");

                e.HasKey(x => x.Id);

                e.Property(x => x.Action)
                    .HasMaxLength(32)
                    .IsRequired();

                e.Property(x => x.Reason)
                    .HasMaxLength(1000);

                e.Property(x => x.CreatedAt)
                    .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

                e.HasOne(x => x.Request)
                    .WithMany(r => r.Decisions)            // add Decisions nav on OnboardingRequest (see next)
                    .HasForeignKey(x => x.RequestId)
                    .OnDelete(DeleteBehavior.Cascade);     // delete log when request is deleted (usually you keep requests)
            });

        }
    }
}
