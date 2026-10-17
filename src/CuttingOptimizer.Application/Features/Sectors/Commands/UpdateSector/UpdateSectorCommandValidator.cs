using FluentValidation;

namespace CuttingOptimizer.Application.Features.Sectors.Commands.UpdateSector;

public sealed class UpdateSectorCommandValidator
    : AbstractValidator<UpdateSectorCommand>
{
    public UpdateSectorCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(50)
            .Matches(@"^[a-zA-Z0-9_-]+$");

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);
    }
}