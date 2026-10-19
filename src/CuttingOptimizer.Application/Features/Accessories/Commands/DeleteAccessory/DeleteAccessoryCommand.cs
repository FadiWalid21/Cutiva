using MediatR;

namespace CuttingOptimizer.Application.Features.Accessories.Commands.DeleteAccessory;

public sealed record DeleteAccessoryCommand(Guid Id)
    : IRequest<DeleteAccessoryResult>;

public enum DeleteAccessoryResult
{
    Deleted,
    Deactivated,
    NotFound
}
