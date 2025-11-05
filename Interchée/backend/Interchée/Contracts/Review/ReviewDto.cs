using System.ComponentModel.DataAnnotations;

namespace Interchée.Contracts.Review
{
    // Schedule a review
    public record ReviewScheduleDto(
        [Required] Guid UserId,
        [Required] DateTime ScheduledAt
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
        List<ReviewFeedbackReadDto>? Feedbacks
    );

    public record ReviewFeedbackReadDto(
        long Id,
        string Category,
        string Feedback,
        DateTime CreatedAt
    );
    public record ReviewUpdateDto(
    [Required] DateTime ScheduledAt
);
}