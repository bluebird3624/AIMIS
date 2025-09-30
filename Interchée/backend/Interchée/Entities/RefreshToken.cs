namespace Interchée.Entities
{
    public class RefreshToken
    {
        public long Id { get; set; }
        public Guid UserId { get; set; }
        public string Token { get; set; } = default!;
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? RevokedAt { get; set; }
        public string? CreatedByIp { get; set; }

        public AppUser? User { get; set; }
    }
}
