using System.ComponentModel.DataAnnotations;

namespace CuttingOptimizer.Api.Contracts.CuttingPlans;

public sealed class GenerateCuttingPlanRequest
{
    [Required]
    public Guid MaterialId { get; init; }

    [Required]
    [Range(0, int.MaxValue)]
    public int? KerfMm { get; init; }

    [Required]
    [MinLength(1)]
    public IReadOnlyList<Guid> RequiredCutIds { get; init; } = [];

    /// <summary>Include remnant warehouse stock (default true).</summary>
    public bool UseRemnants { get; init; } = true;
}
