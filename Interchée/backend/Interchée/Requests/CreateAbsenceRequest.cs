using System.ComponentModel.DataAnnotations;

public class CreateAbsenceRequest
{
    [Required]
    public string? Reason { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    public string? DocumentationUrl { get; set; } // For future file upload support
}

// Application/Requests/ApproveAbsenceRequest.cs
public class ApproveAbsenceRequest
{
    public bool IsApproved { get; set; }
    public string? RejectionReason { get; set; }
}
