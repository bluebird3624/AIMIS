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

        // === EXISTING DbSets ===
        public DbSet<Department> Departments => Set<Department>();
        public DbSet<DepartmentRoleAssignment> DepartmentRoleAssignments => Set<DepartmentRoleAssignment>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        // === NEW DbSets for Learning Module ===
        public DbSet<Assignment> Assignments => Set<Assignment>();
        public DbSet<AssignmentSubmission> AssignmentSubmissions => Set<AssignmentSubmission>();
        public DbSet<AssignmentAttachment> AssignmentAttachments => Set<AssignmentAttachment>();
        public DbSet<Grade> Grades => Set<Grade>();
        public DbSet<Feedback> Feedbacks => Set<Feedback>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);

            // === EXISTING Configurations ===

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

            // === NEW Configurations for Learning Module ===

            // Assignment
            b.Entity<Assignment>(e =>
            {
                e.Property(x => x.Title).HasMaxLength(256).IsRequired();
                e.Property(x => x.Description).HasMaxLength(4000);

                e.HasOne(x => x.CreatedBy)
                    .WithMany()
                    .HasForeignKey(x => x.CreatedById)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.Department)
                    .WithMany()
                    .HasForeignKey(x => x.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // AssignmentAttachment
            b.Entity<AssignmentAttachment>(e =>
            {
                e.Property(x => x.FileName).HasMaxLength(512).IsRequired();
                e.Property(x => x.FilePath).HasMaxLength(1024).IsRequired();

                e.HasOne(x => x.Assignment)
                    .WithMany(a => a.Attachments)
                    .HasForeignKey(x => x.AssignmentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // AssignmentSubmission
            b.Entity<AssignmentSubmission>(e =>
            {
                e.Property(x => x.GitRepositoryUrl).HasMaxLength(2048).IsRequired();
                e.Property(x => x.LastCommitHash).HasMaxLength(64);
                e.Property(x => x.BranchName).HasMaxLength(256);
                e.Property(x => x.CommitHistoryJson).HasMaxLength(8000);

                // Unique constraint: One submission per assignment per intern
                e.HasIndex(x => new { x.AssignmentId, x.InternId }).IsUnique();

                e.HasOne(x => x.Assignment)
                    .WithMany(a => a.Submissions)
                    .HasForeignKey(x => x.AssignmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.Intern)
                    .WithMany()
                    .HasForeignKey(x => x.InternId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Grade
            b.Entity<Grade>(e =>
            {
                e.Property(x => x.Comments).HasMaxLength(2000);
                e.Property(x => x.RubricEvaluationJson).HasMaxLength(4000);

                e.Property(x => x.Score).HasPrecision(5, 2);

                e.HasOne(x => x.Submission)
                    .WithOne(s => s.Grade)
                    .HasForeignKey<Grade>(x => x.SubmissionId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.GradedBy)
                    .WithMany()
                    .HasForeignKey(x => x.GradedById)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Feedback
            b.Entity<Feedback>(e =>
            {
                e.Property(x => x.Comment).HasMaxLength(2000).IsRequired();

                e.HasOne(x => x.Submission)
                    .WithMany(s => s.Feedbacks)
                    .HasForeignKey(x => x.SubmissionId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.Supervisor)
                    .WithMany()
                    .HasForeignKey(x => x.SupervisorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}