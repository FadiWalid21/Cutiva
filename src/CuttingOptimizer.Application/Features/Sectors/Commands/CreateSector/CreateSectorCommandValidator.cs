using FluentValidation;

namespace CuttingOptimizer.Application.Features.Sectors.Commands.CreateSector;


public sealed class CreateSectorCommandValidator
    : AbstractValidator<CreateSectorCommand>
{
    public CreateSectorCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(50)
            .Matches(@"^[a-zA-Z0-9_-]+$");

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);
    }
}