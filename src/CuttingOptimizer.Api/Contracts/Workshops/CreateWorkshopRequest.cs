using System.ComponentModel.DataAnnotations;

namespace CuttingOptimizer.Api.Contracts.Workshops;

public sealed record CreateWorkshopRequest(
    [Required]
    [StringLength(150, MinimumLength = 1)]
    string Name,

    [Range(0.01, double.MaxValue)]
    decimal? ProductionPricePerM2 = null);
