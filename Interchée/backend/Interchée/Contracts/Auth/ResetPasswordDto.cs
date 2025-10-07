using System.ComponentModel.DataAnnotations;

namespace Interchée.Contracts.Auth
{
    public record ResetPasswordDto(
        [Required, EmailAddress] string Email,
        [Required] string Token,                   // URL-decoded on client or send raw and encode in link
        [Required, MinLength(8)] string NewPassword
    );
}
