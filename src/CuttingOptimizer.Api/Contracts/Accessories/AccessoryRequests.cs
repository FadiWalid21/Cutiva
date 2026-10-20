using System.ComponentModel.DataAnnotations;
using CuttingOptimizer.Domain.Enums;

namespace CuttingOptimizer.Api.Contracts.Accessories;

public sealed record CreateAccessoryRequest(
    [Required]
    [StringLength(200, MinimumLength = 1)]
    string Name,

    [Required]
    [Range(typeof(decimal), "0.01", "999999999", ParseLimitsInInvariantCulture = true)]
    decimal UnitPrice,

    [StringLength(100)]
    string? Brand = null,

    AccessoryUnit Unit = AccessoryUnit.Piece);

public sealed record UpdateAccessoryRequest(
    [Required]
    [StringLength(200, MinimumLength = 1)]
    string Name,

    [Required]
    [Range(typeof(decimal), "0.01", "999999999", ParseLimitsInInvariantCulture = true)]
    decimal UnitPrice,

    [StringLength(100)]
    string? Brand = null,

    AccessoryUnit Unit = AccessoryUnit.Piece,

    bool IsActive = true);
