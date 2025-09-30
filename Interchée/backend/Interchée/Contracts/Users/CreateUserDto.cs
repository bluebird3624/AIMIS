using System.ComponentModel.DataAnnotations;

namespace Interchée.Contracts.Users
{
    public record CreateUserDto(
        [property: Required, MinLength(3)] string UserName,
        [property: Required, EmailAddress] string Email,
        [property: Required, MinLength(8)] string Password,

        // NEW: structured names
        [property: Required, MaxLength(64)] string FirstName,
        [property: Required, MaxLength(64)] string LastName,
        [property: MaxLength(64)] string? MiddleName
    );
}
