using System.ComponentModel.DataAnnotations;

namespace CuttingOptimizer.Api.Contracts.RequiredCuts;

public sealed class CreateRequiredCutRequest
{
    [Required]
    public Guid ProjectSectionId { get; init; }

    [Required]
    public Guid MaterialId { get; init; }

    [Required]
    [StringLength(150, MinimumLength = 1)]
    public string Name { get; init; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int LengthMm { get; init; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; init; }
}