using Microsoft.AspNetCore.Identity;

namespace CuttingOptimizer.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }
}
