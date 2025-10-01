using System.ComponentModel.DataAnnotations;

namespace Interchée.Contracts.Users
{
    /// <summary>Admin/HR direct user creation.</summary>
    public record CreateUserDto(
        [Required, MinLength(3, ErrorMessage = "UserName must be at least 3 characters.")]
        string UserName,

        [Required, EmailAddress(ErrorMessage = "A valid email address is required.")]
        string Email,

        [Required, MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        string Password,

        // Structured names
        [Required, MaxLength(64)]
        string FirstName,

        [Required, MaxLength(64)]
        string LastName,

        [MaxLength(64)]
        string? MiddleName
    );
}
