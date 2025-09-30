namespace Interchée.Services.Interfaces
{
    public class GitRepositoryMetadata
    {
        public string LastCommitHash { get; set; } = default!;
        public string CommitHistoryJson { get; set; } = default!;
        public string DefaultBranch { get; set; } = default!;
        public int RepositorySize { get; set; }
    }

    public interface IGitIntegrationService
    {
        Task<bool> ValidateGitUrlAsync(string gitUrl);
        Task<GitRepositoryMetadata> GetRepositoryMetadataAsync(string gitUrl);
    }
}