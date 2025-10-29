namespace Interchée.Contracts.Webhooks
{
    public class Commit
    {
        public string id { get; set; } = default!;
        public string message { get; set; } = default!;
        public DateTime timestamp { get; set; }
        public string url { get; set; } = default!;
        public Author author { get; set; } = default!;
        public Committer committer { get; set; } = default!;
    }
}
