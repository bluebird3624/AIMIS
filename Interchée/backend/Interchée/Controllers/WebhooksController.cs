using Interchée.Contracts.Webhooks;
using Interchée.Data;
using Interchée.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Interchée.Controllers
{
    [ApiController]
    [Route("webhooks")]
    public class WebhooksController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ILogger<WebhooksController> _logger;

        public WebhooksController(AppDbContext db, ILogger<WebhooksController> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <summary>GitHub webhook for automatic commit tracking</summary>
        [HttpPost("github")]
        public async Task<IActionResult> GitHubWebhook([FromBody] GitHubWebhookPayload payload)
        {
            try
            {
                _logger.LogInformation("Received GitHub webhook for repository: {RepoUrl}", payload.repository.html_url);

                // Only process push events to main branches
                if (payload.@ref.StartsWith("refs/heads/") && payload.commits?.Count > 0)
                {
                    var branch = payload.@ref.Replace("refs/heads/", "");

                    // Find submission by repo URL
                    var submission = await _db.AssignmentSubmissions
                        .FirstOrDefaultAsync(s => s.RepoUrl == payload.repository.html_url && s.Branch == branch);

                    if (submission != null)
                    {
                        _logger.LogInformation("Found submission {SubmissionId} for repo {RepoUrl}", submission.Id, payload.repository.html_url);

                        // Add new commits
                        foreach (var commit in payload.commits)
                        {
                            var existingCommit = await _db.SubmissionCommits
                                .AnyAsync(c => c.Sha == commit.id && c.SubmissionId == submission.Id);

                            if (!existingCommit)
                            {
                                _db.SubmissionCommits.Add(new SubmissionCommit
                                {
                                    SubmissionId = submission.Id,
                                    Sha = commit.id,
                                    Message = commit.message,
                                    AuthorEmail = commit.author.email,
                                    CommittedAt = commit.timestamp
                                });

                                _logger.LogInformation("Added commit {CommitSha} to submission {SubmissionId}", commit.id, submission.Id);
                            }
                        }

                        // Update latest commit
                        submission.LatestCommitSha = payload.after;
                        await _db.SaveChangesAsync();

                        _logger.LogInformation("Updated submission {SubmissionId} with latest commit {CommitSha}", submission.Id, payload.after);
                    }
                    else
                    {
                        _logger.LogWarning("No submission found for repository: {RepoUrl}", payload.repository.html_url);
                    }
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing GitHub webhook");
                return StatusCode(500, "Error processing webhook");
            }
        }

        /// <summary>Manual webhook test endpoint</summary>
        [HttpPost("test")]
        public async Task<IActionResult> TestWebhook([FromBody] TestWebhookDto dto)
        {
            var submission = await _db.AssignmentSubmissions
                .FirstOrDefaultAsync(s => s.Id == dto.SubmissionId);

            if (submission == null) return NotFound("Submission not found");

            // Simulate a commit
            var commit = new SubmissionCommit
            {
                SubmissionId = dto.SubmissionId,
                Sha = Guid.NewGuid().ToString("N")[..8],
                Message = dto.CommitMessage ?? "Test commit via webhook",
                AuthorEmail = "test@example.com",
                CommittedAt = DateTime.UtcNow
            };

            submission.LatestCommitSha = commit.Sha;
            _db.SubmissionCommits.Add(commit);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Test commit added", commitSha = commit.Sha });
        }
    }

    public class TestWebhookDto
    {
        public long SubmissionId { get; set; }
        public string? CommitMessage { get; set; }
    }
}