namespace Interchée.Dtos
{
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
