using FluentValidation;

namespace CuttingOptimizer.Application.Features.Materials.Commands.CreateMaterial;

public sealed class CreateMaterialCommandValidator
    : AbstractValidator<CreateMaterialCommand>
{
    public CreateMaterialCommandValidator()
    {
        RuleFor(x => x.SectorId)
            .NotEmpty();

        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(50)
            .Matches(@"^[a-zA-Z0-9_-]+$");

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(x => x.Description)
            .MaximumLength(500);
    }
}