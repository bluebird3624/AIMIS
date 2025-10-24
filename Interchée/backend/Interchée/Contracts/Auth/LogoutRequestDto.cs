using System.ComponentModel.DataAnnotations;

namespace Interchée.Contracts.Auth
{
    /// <summary>
    /// Revoke a refresh token (logs this session out).
    /// </summary>
    public record LogoutRequestDto(
        [Required, MinLength(32)] string RefreshToken
    );
}
