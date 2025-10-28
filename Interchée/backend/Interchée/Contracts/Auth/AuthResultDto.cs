namespace Interchée.Contracts.Auth
{
    /// <summary>
    /// Auth response: short-lived access token + optional stored refresh token.
    /// </summary>
    public record AuthResultDto(
        string AccessToken,
        string? RefreshToken,
        DateTimeOffset ExpiresAtUtc
    );
}
