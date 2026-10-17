using System.ComponentModel.DataAnnotations;

namespace CuttingOptimizer.Api.Contracts.Workshops;

public sealed record CreateWorkshopRequest(
    [Required]
    [StringLength(150, MinimumLength = 1)]
    string Name);