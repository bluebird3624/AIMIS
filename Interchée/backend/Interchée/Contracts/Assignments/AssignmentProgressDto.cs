namespace Interchée.Contracts.Assignments
{
    public record AssignmentProgressDto(
       int TotalAssignees,
       int SubmittedCount,
       int ReviewedCount,
       int InProgressCount,
       int NotStartedCount,
       double SubmissionRate,
       double ReviewRate
   );
}
