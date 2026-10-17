using FluentValidation;

namespace CuttingOptimizer.Application.Features.Sectors.Commands.DeleteSector;

public sealed class DeleteSectorCommandValidator
    : AbstractValidator<DeleteSectorCommand>
{
    public DeleteSectorCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}