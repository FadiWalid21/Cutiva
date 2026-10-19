using System.ComponentModel.DataAnnotations;

namespace CuttingOptimizer.Api.Contracts.ProjectSections;

public sealed record CreateProjectSectionRequest(
    [Required]
    Guid SectorId,

    [Required]
    [StringLength(200, MinimumLength = 1)]
    string Name,

    [Required]
    [Range(1, int.MaxValue)]
    int HeightMm,

    [Required]
    [Range(1, int.MaxValue)]
    int WidthMm,

    [Required]
    [Range(1, int.MaxValue)]
    int UnitQuantity,

    IReadOnlyList<SectionAccessoryRequest>? Accessories = null);

public sealed record SectionAccessoryRequest(
    [Required]
    Guid AccessoryId,

    [Required]
    [Range(1, int.MaxValue)]
    int Quantity);
