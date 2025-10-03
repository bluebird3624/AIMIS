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
        public virtual ICollection<Intern> SupervisedInterns { get; set; } = [];
        public virtual ICollection<AbsenceRequest> ApprovedAbsenceRequests { get; set; } = [];
        public object Department => $"{FirstName} {LastName}";
        public int? DepartmentId { get; internal set; }
    }
}