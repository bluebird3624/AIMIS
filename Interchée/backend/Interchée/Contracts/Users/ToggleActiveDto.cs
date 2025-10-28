using System.ComponentModel.DataAnnotations;

namespace Interchée.Contracts.Users
{
    public record ToggleActiveDto([property: Required] bool IsActive);

}
