using FluentValidation;

namespace CuttingOptimizer.Application.Features.StockBars.Commands.CreateStockBar;

public sealed class CreateStockBarValidator
    : AbstractValidator<CreateStockBarCommand>
{
    public CreateStockBarValidator()
    {
        RuleFor(x => x.MaterialId)
            .NotEmpty();

        RuleFor(x => x.LengthMm)
            .GreaterThan(0);

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0);

        RuleFor(x => x.AvailableQuantity)
            .GreaterThanOrEqualTo(0);
    }
}