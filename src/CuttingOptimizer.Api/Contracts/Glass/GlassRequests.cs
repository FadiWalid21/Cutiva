using System.ComponentModel.DataAnnotations;

namespace CuttingOptimizer.Api.Contracts.Glass;

public sealed record CreateGlassTypeRequest(
    [Required]
    [StringLength(150, MinimumLength = 1)]
    string Name);

public sealed record UpdateGlassTypeRequest(
    [Required]
    [StringLength(150, MinimumLength = 1)]
    string Name,

    bool IsActive = true);

public sealed record CreateGlassSpecRequest(
    [Required]
    [Range(typeof(decimal), "0.01", "999999999", ParseLimitsInInvariantCulture = true)]
    decimal ThicknessMm,

    [Required]
    [Range(typeof(decimal), "0", "999999999", ParseLimitsInInvariantCulture = true)]
    decimal PricePerSqm,

    [StringLength(200)]
    string? Description = null);

public sealed record UpdateGlassSpecRequest(
    [Required]
    [Range(typeof(decimal), "0.01", "999999999", ParseLimitsInInvariantCulture = true)]
    decimal ThicknessMm,

    [Required]
    [Range(typeof(decimal), "0", "999999999", ParseLimitsInInvariantCulture = true)]
    decimal PricePerSqm,

    [StringLength(200)]
    string? Description = null,

    bool IsActive = true);
