using Interchée.Data;
using Interchée.Entities;
using Microsoft.EntityFrameworkCore;

namespace Interchée.Auth
{
    /// <summary>
    /// Handles server-side refresh token validation and revocation.
    /// </summary>
    public class RefreshTokenService
    {
        private readonly AppDbContext _db;

        public RefreshTokenService(AppDbContext db) => _db = db;

        /// <summary>
        /// Returns a valid (non-expired, non-revoked) token, or null.
        /// </summary>
        public async Task<RefreshToken?> GetValidAsync(string token)
        {
            var now = DateTime.UtcNow;
            return await _db.RefreshTokens.AsNoTracking()
                .FirstOrDefaultAsync(t => t.Token == token && t.ExpiresAt > now && t.RevokedAt == null);
        }

        /// <summary>
        /// Marks a refresh token as revoked. Idempotent.
        /// </summary>
        public async Task RevokeAsync(string token)
        {
            var t = await _db.RefreshTokens.FirstOrDefaultAsync(x => x.Token == token);
            if (t == null) return;
            if (t.RevokedAt == null)
            {
                t.RevokedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }
        }
    }
}
