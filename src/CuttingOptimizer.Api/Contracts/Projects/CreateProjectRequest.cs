using System.ComponentModel.DataAnnotations;

namespace CuttingOptimizer.Api.Contracts.Projects;

public sealed record CreateProjectRequest(
    [Required]
    [StringLength(200, MinimumLength = 1)]
    string Name,

    [StringLength(150)]
    string? CustomerName);