using System.ComponentModel.DataAnnotations;

namespace CuttingOptimizer.Api.Contracts.Materials;


public sealed record CreateMaterialRequest(
    [Required]
    Guid SectorId,

    [Required]
    [StringLength(50, MinimumLength = 1)]
    [RegularExpression(@"^[a-zA-Z0-9_-]+$")]
    string Code,

    [Required]
    [StringLength(150, MinimumLength = 1)]
    string Name,

    [StringLength(500)]
    string? Description);