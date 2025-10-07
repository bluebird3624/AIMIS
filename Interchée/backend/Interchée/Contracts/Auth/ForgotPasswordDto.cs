using System.ComponentModel.DataAnnotations;

namespace Interchée.Contracts.Auth
{
    public record ForgotPasswordDto(
         [Required, EmailAddress] string Email
     );
}
