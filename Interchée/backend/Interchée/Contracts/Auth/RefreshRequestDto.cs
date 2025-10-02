using System.ComponentModel.DataAnnotations;

namespace Interchée.Contracts.Auth
{
    /// <summary>
    /// Exchange a valid refresh token for a new access token.
    /// </summary>
    public record RefreshRequestDto(
        [Required, MinLength(32)] string RefreshToken
    );
}
