using Interchée.Entities;

namespace Interchée.Auth
{
    /// <summary>
    /// Issues signed JWT access tokens and creates persisted refresh tokens.
    /// </summary>
    public interface IJwtTokenService
    {
        /// <summary>Creates a signed JWT access token for the given user.</summary>
        Task<(string accessToken, DateTime expiresUtc)> CreateAccessTokenAsync(AppUser user);

        /// <summary>Creates and stores a new refresh token for the user.</summary>
        Task<string> CreateRefreshTokenAsync(AppUser user, string? ip = null);
    }
}
