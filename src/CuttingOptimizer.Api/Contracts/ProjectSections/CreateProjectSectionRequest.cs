using System.ComponentModel.DataAnnotations;

namespace CuttingOptimizer.Api.Contracts.ProjectSections;

public sealed record CreateProjectSectionRequest(
    [Required]
    Guid SectorId,

    [Required]
    [StringLength(200, MinimumLength = 1)]
    string Name);