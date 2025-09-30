using Microsoft.AspNetCore.Identity;

namespace Interchée.Entities
{
    public class AppUser : IdentityUser<Guid>
    {
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string FirstName { get; set; } = string.Empty;   // <= 64
        public string LastName { get; set; } = string.Empty;   // <= 64
        public string? MiddleName { get; set; }
    }
}
