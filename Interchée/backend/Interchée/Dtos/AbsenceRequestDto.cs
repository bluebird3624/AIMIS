using Interchée.Entities;

namespace Interchée.Dtos
{
    public class AbsenceRequestDto
    {
        public int Id { get; set; }
        public int InternId { get; set; }
        public string? InternName { get; set; }
        public string? InternEmail { get; set; }
        public string? Reason { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public AbsenceStatus Status { get; set; }
        public string? RejectionReason { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public int TotalDays { get; set; }
    }
}
