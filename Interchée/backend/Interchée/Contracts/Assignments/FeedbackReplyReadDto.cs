namespace Interchée.Contracts.Assignments
{
    public record FeedbackReplyReadDto(
           long Id,
           long FeedbackId,
           string Message,
           Guid CreatedByUserId,
           string CreatedByUserName,
           DateTime CreatedAt
    );
   
}
