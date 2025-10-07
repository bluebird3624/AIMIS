using System.ComponentModel.DataAnnotations;

namespace Interchée.Contracts.Auth
{
    public record ChangePasswordDto(
        [Required, MinLength(8)] string CurrentPassword,
        [Required, MinLength(8)] string NewPassword
    );
}
