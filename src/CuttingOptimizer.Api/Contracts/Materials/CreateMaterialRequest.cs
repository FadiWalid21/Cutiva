using System.ComponentModel.DataAnnotations;
using CuttingOptimizer.Domain.Enums;

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
    string? Description,

    MaterialCategory Category = MaterialCategory.Aluminum,

    [Range(typeof(decimal), "0.01", "9999", ParseLimitsInInvariantCulture = true)]
    decimal? ThicknessMm = null);

public sealed record UpdateMaterialRequest(
    [Required]
    [StringLength(150, MinimumLength = 1)]
    string Name,

    [StringLength(500)]
    string? Description,

    MaterialCategory Category = MaterialCategory.Aluminum,

    [Range(typeof(decimal), "0.01", "9999", ParseLimitsInInvariantCulture = true)]
    decimal? ThicknessMm = null,

    bool IsActive = true);
