using FluentValidation;

namespace CuttingOptimizer.Application.Features.StockBars.Commands.UpdateStockBar;

public sealed class UpdateStockBarCommandValidator
    : AbstractValidator<UpdateStockBarCommand>
{
    public UpdateStockBarCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.LengthMm)
            .GreaterThan(0);

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0);
    }
}
