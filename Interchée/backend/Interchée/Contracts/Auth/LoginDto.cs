using System.ComponentModel.DataAnnotations;

namespace Interchée.Contracts.Auth
{
    /// Login request. We use Email for sign-in.
    /// </summary>
    /// <summary>
    /// Login request. We use Email for sign-in.
    /// </summary>
    public record LoginDto(
        [Required, EmailAddress(ErrorMessage = "A valid email address is required.")] string Email,
        [Required, MinLength(8, ErrorMessage = "Password must be at least 8 characters.")] string Password
    );

}
