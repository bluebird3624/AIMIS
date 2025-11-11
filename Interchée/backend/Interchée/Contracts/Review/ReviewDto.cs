using System.ComponentModel.DataAnnotations;

namespace Interchée.Contracts.Review
{
    // Schedule a review
    public record ReviewScheduleDto(
        [Required] Guid UserId,
        [Required] DateTime ScheduledAt,
        [Required, MaxLength(200)] string Title,
        [MaxLength(1000)] string? Description,
        [MaxLength(100)] string? Location
    );

    // Submit review scores and feedback
    public record ReviewSubmitDto(
        [Range(0, 5)] decimal TeamworkScore,
        [Range(0, 5)] decimal CommunicationScore,
        [Range(0, 5)] decimal TechnicalSkillsScore,
        [Range(0, 5)] decimal InitiativeScore,
        [Range(0, 5)] decimal ProfessionalismScore,
        [MaxLength(1000)] string? OverallComments,
        List<ReviewFeedbackDto>? Feedbacks
    );

    public record ReviewFeedbackDto(
        [Required] string Category,
        [Required, MaxLength(500)] string Feedback
    );

    // Read DTO
    public record ReviewReadDto(
        long Id,
        Guid UserId,
        Guid SupervisorId,
        int DepartmentId,
        string? Title,
        string? Description,
        string? Location,
        DateTime ScheduledAt,
        DateTime? ConductedAt,
        string Status,
        decimal TeamworkScore,
        decimal CommunicationScore,
        decimal TechnicalSkillsScore,
        decimal InitiativeScore,
        decimal ProfessionalismScore,
        decimal OverallScore,
        string? OverallComments,
        DateTime CreatedAt,
        List<ReviewFeedbackReadDto>? Feedbacksp
    );

    public record ReviewFeedbackReadDto(
        long Id,
        string Category,
        string Feedback,
        DateTime CreatedAt
    );
    public record ReviewUpdateDto(
    [Required] DateTime ScheduledAt,
    [MaxLength(200)] string? Title,
    [MaxLength(1000)] string? Description,
    [MaxLength(100)] string? Location
);
    // Add to your Review DTOs
    public record BulkReviewScheduleDto(
        [Required, MinLength(1)] List<Guid> UserIds, // Multiple Attaché IDs
        [Required] DateTime ScheduledAt,
        [Required, MaxLength(200)] string Title,
        [MaxLength(1000)] string? Description,
        [MaxLength(100)] string? Location
    );
}