namespace Interchée.Contracts.Assignments
{
    public record FeedbackReadDto(
           long Id,
           string Title,
           string Message,
           Guid CreatedByUserId,
           string CreatedByUserName,
            DateTime CreatedAt,
           List<FeedbackReplyReadDto> Replies
    );
}
