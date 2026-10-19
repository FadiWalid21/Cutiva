using Microsoft.AspNetCore.Identity;

namespace CuttingOptimizer.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }

    /// <summary>Workshop this user belongs to; null for platform admins.</summary>
    public Guid? WorkshopId { get; set; }

    public bool IsPrimaryContact { get; set; }
}
