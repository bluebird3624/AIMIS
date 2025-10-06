using System.Text.RegularExpressions;

namespace Interchée.Services
{
    public class SimpleGitService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<SimpleGitService> _logger;

        public SimpleGitService(HttpClient httpClient, ILogger<SimpleGitService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public bool ValidateRepoUrl(string repoUrl)
        {
            if (string.IsNullOrWhiteSpace(repoUrl))
                return false;

            try
            {
                if (!Uri.TryCreate(repoUrl, UriKind.Absolute, out var uri))
                    return false;

                // Validate GitHub or GitLab URL pattern
                var isValidGitHub = uri.Host == "github.com" &&
                                  uri.AbsolutePath.Split('/').Length >= 3;

                var isValidGitLab = uri.Host == "gitlab.com" &&
                                  uri.AbsolutePath.Split('/').Length >= 3;

                return isValidGitHub || isValidGitLab;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to validate repo URL: {RepoUrl}", repoUrl);
                return false;
            }
        }

        public string? ExtractRepoInfo(string repoUrl)
        {
            try
            {
                if (Uri.TryCreate(repoUrl, UriKind.Absolute, out var uri))
                {
                    var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
                    if (segments.Length >= 2)
                    {
                        var owner = segments[0];
                        var repo = segments[1].Replace(".git", "");
                        return $"{owner}/{repo}";
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to extract repo info from: {RepoUrl}", repoUrl);
            }

            return null;
        }
    }
}