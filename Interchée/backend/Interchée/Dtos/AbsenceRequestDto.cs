using System.ComponentModel.DataAnnotations;

namespace Interchée.Dtos
{
    public class CreateAbsenceRequestDto
    {
        [Required(ErrorMessage = "Reason is required")]
        [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
        public string? Reason { get; set; }

        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required")]
        public DateTime EndDate { get; set; }
    }

    public class ApproveAbsenceRequestDto
    {
        [Required(ErrorMessage = "Approval status is required")]
        public bool IsApproved { get; set; }

        [StringLength(500, ErrorMessage = "Rejection reason cannot exceed 500 characters")]
        public string? RejectionReason { get; set; }
    }

    public class AbsenceRequestDto
    {
        public int Id { get; set; }
        public int InternId { get; set; }
        public string? InternName { get; set; }
        public string? InternEmail { get; set; }
        public string? Reason { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Status { get; set; }
        public string? RejectionReason { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public int TotalDays { get; set; }
    }

    public class AbsenceSummaryDto
    {
        public int InternId { get; set; }
        public string? InternName { get; set; }
        public int TotalAbsenceDays { get; set; }
        public int PendingRequests { get; set; }
        public int ApprovedRequests { get; set; }
        public int RejectedRequests { get; set; }
        public int RemainingAbsenceDays { get; set; }
    }
}