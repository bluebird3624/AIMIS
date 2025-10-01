using System.ComponentModel.DataAnnotations;

namespace Interchée.Contracts.Users
{
    /// <summary>
    /// Admin/HR direct user creation.
    /// </summary>
    public record CreateUserDto(
        [property: Required, MinLength(3, ErrorMessage = "UserName must be at least 3 characters.")]
        string UserName,

        [property: Required, EmailAddress(ErrorMessage = "A valid email address is required.")]
        string Email,

        [property: Required, MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        string Password,

    // NEW: structured names
    [property: Required, MaxLength(64)] string FirstName,
        [property: Required, MaxLength(64)] string LastName,
        [property: MaxLength(64)] string? MiddleName
    );
}
