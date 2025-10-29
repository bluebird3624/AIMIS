namespace Interchée.Contracts.Webhooks
{
    public class GitHubWebhookPayload
    {
        public string @ref { get; set; } = default!;
        public string before { get; set; } = default!;
        public string after { get; set; } = default!;
        public Repository repository { get; set; } = default!;
        public List<Commit> commits { get; set; } = new();
        public Pusher pusher { get; set; } = default!;
    }
}
