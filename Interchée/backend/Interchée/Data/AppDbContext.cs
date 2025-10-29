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

        //Absence Management

        public DbSet<AbsenceRequest> AbsenceRequests => Set<AbsenceRequest>();
        public DbSet<AbsenceDecision> AbsenceDecisions => Set<AbsenceDecision>();
        public DbSet<AbsenceLimitPolicy> AbsenceLimitPolicies => Set<AbsenceLimitPolicy>();

        public DbSet<Assignment> Assignments => Set<Assignment>();
        public DbSet<AssignmentAssignee> AssignmentAssignees => Set<AssignmentAssignee>();
        public DbSet<AssignmentSubmission> AssignmentSubmissions => Set<AssignmentSubmission>();
        public DbSet<SubmissionCommit> SubmissionCommits => Set<SubmissionCommit>();
        public DbSet<Grade> Grades => Set<Grade>();
        public DbSet<FeedbackComment> FeedbackComments => Set<FeedbackComment>();

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

            // Assignment
            b.Entity<Assignment>(e =>
            {
                e.Property(x => x.Title).HasMaxLength(160).IsRequired();
                e.Property(x => x.Status).HasMaxLength(32).IsRequired();

                // Indexes for performance
                e.HasIndex(x => x.DepartmentId);
                e.HasIndex(x => new { x.DepartmentId, x.Status });

                // Relationships
                e.HasOne(x => x.Department)
                    .WithMany()
                    .HasForeignKey(x => x.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict); // Keep assignments if department is deleted

                e.HasOne(x => x.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(x => x.CreatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict); // Keep assignments if user is deleted
            });

            // AssignmentAssignee (Junction table)
            b.Entity<AssignmentAssignee>(e =>
            {
                // Unique constraint: user can't be assigned to same assignment multiple times
                e.HasIndex(x => new { x.AssignmentId, x.UserId }).IsUnique();

                // Relationships
                e.HasOne(x => x.Assignment)
                    .WithMany(a => a.Assignees)
                    .HasForeignKey(x => x.AssignmentId)
                    .OnDelete(DeleteBehavior.Cascade); // Remove assignments if assignment deleted

                e.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict); // Keep assignment history if user is deleted
            });

            // AssignmentSubmission
            b.Entity<AssignmentSubmission>(e =>
            {
                e.Property(x => x.RepoUrl).HasMaxLength(512);
                e.Property(x => x.Branch).HasMaxLength(120);
                e.Property(x => x.LatestCommitSha).HasMaxLength(64);
                e.Property(x => x.Status).HasMaxLength(32).IsRequired();

                // Unique constraint: one submission per assignment per user
                e.HasIndex(x => new { x.AssignmentId, x.UserId }).IsUnique();

                // Performance indexes
                e.HasIndex(x => x.AssignmentId);
                e.HasIndex(x => new { x.AssignmentId, x.Status });

                // Relationships
                e.HasOne(x => x.Assignment)
                    .WithMany(a => a.Submissions)
                    .HasForeignKey(x => x.AssignmentId)
                    .OnDelete(DeleteBehavior.Cascade); // Remove submissions if assignment deleted

                e.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict); // Keep submission history if user is deleted
            });

            // SubmissionCommit
            b.Entity<SubmissionCommit>(e =>
            {
                e.Property(x => x.Sha).HasMaxLength(64).IsRequired();
                e.Property(x => x.AuthorEmail).HasMaxLength(256);

                // Unique constraint: same commit can't be recorded multiple times for same submission
                e.HasIndex(x => new { x.SubmissionId, x.Sha }).IsUnique();

                // Relationship
                e.HasOne(x => x.Submission)
                    .WithMany(s => s.Commits)
                    .HasForeignKey(x => x.SubmissionId)
                    .OnDelete(DeleteBehavior.Cascade); // Remove commits if submission deleted
            });

            // Grade
            b.Entity<Grade>(e =>
            {
                e.Property(x => x.Score).HasPrecision(5, 2);
                e.Property(x => x.MaxScore).HasPrecision(5, 2);

                // Unique constraint: one grade per submission
                e.HasIndex(x => x.SubmissionId).IsUnique();

                // Relationships
                e.HasOne(x => x.Submission)
                    .WithOne(s => s.Grade)
                    .HasForeignKey<Grade>(x => x.SubmissionId)
                    .OnDelete(DeleteBehavior.Cascade); // Remove grade if submission deleted

                e.HasOne(x => x.GradedByUser)
                    .WithMany()
                    .HasForeignKey(x => x.GradedByUserId)
                    .OnDelete(DeleteBehavior.Restrict); // Keep grade history if user is deleted
            });

            // FeedbackComment
            b.Entity<FeedbackComment>(e =>
            {
                e.Property(x => x.Comment).IsRequired();

                // Relationships
                e.HasOne(x => x.Submission)
                    .WithMany(s => s.FeedbackComments)
                    .HasForeignKey(x => x.SubmissionId)
                    .OnDelete(DeleteBehavior.Cascade); // Remove comments if submission deleted

                e.HasOne(x => x.AuthorUser)
                    .WithMany()
                    .HasForeignKey(x => x.AuthorUserId)
                    .OnDelete(DeleteBehavior.Restrict); // Keep comment history if user is deleted
            });

        }

    }

}

