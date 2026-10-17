using System.ComponentModel.DataAnnotations;

namespace CuttingOptimizer.Application.Features.StockBars.Commands.UpdateStockBar;

public sealed class UpdateStockBarRequest
{
    [Required]
    public Guid MaterialId { get; init; }

    [Range(1, int.MaxValue)]
    public int LengthMm { get; init; }

    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    public decimal UnitPrice { get; init; }

    [Range(0, int.MaxValue)]
    public int AvailableQuantity { get; init; }
}