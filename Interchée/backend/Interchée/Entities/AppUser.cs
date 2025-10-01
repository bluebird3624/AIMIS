using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Interchée.Entities
{
    public class AppUser : IdentityUser<Guid>
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? MiddleName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Navigation properties for Absence Management Module
        public virtual ICollection<Intern> SupervisedInterns { get; set; } = new List<Intern>();
        public virtual ICollection<AbsenceRequest> ApprovedAbsenceRequests { get; set; } = new List<AbsenceRequest>();

        // Computed properties
        [NotMapped]
        public string FullName => MiddleName != null
            ? $"{FirstName} {MiddleName} {LastName}"
            : $"{FirstName} {LastName}";

        [NotMapped]
        public string DisplayName => $"{FirstName} {LastName}";

        public object? Department { get; internal set; }
        public int? DepartmentId { get; internal set; }
        public object? DepartmentName { get; internal set; }
    }
}