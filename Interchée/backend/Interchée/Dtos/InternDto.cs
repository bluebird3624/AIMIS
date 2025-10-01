using Interchée.Entities;

namespace Interchée.Dtos
{
    public class InternDto
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? FullName { get; set; }
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public string? SupervisorId { get; set; }
        public string? SupervisorName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? University { get; set; }
        public string? CourseOfStudy { get; set; }
        public string? Status { get; set; }
        public int TotalAbsenceRequests { get; set; }
        public int PendingAbsenceRequests { get; set; }
        public int ApprovedAbsenceDays { get; set; }
    }

    public class CreateInternDto
    {
        public string? UserId { get; set; }
        public string? SupervisorId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? University { get; set; }
        public string? CourseOfStudy { get; set; }
    }

    public class UpdateInternDto
    {
        public string? SupervisorId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? University { get; set; }
        public string? CourseOfStudy { get; set; }
        public InternStatus Status { get; set; }
    }

    public class UpdateInternSupervisorDto
    {
        public string? SupervisorId { get; set; }
    }
}