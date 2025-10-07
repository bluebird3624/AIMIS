using System.ComponentModel.DataAnnotations;

namespace Interchée.Contracts.Auth
{
    public record ConfirmEmailDto(
        [Required] string UserId,                  // Guid as string
        [Required] string Token                    // Email confirmation token (URL-encoded safe)
    );
}
