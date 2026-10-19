using System.ComponentModel.DataAnnotations;

namespace CuttingOptimizer.Api.Contracts.Sectors;

public sealed record CreateSectorRequest(
    [Required]
    [StringLength(50, MinimumLength = 1)]
    [RegularExpression(@"^[a-zA-Z0-9_-]+$")]
    string Code,

    [Required]
    [StringLength(100, MinimumLength = 1)]
    string Name,

    Guid? MaterialId = null);
