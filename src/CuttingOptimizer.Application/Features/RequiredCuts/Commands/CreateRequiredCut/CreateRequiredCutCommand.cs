using MediatR;

namespace CuttingOptimizer.Application.Features.RequiredCuts.Commands.CreateRequiredCut;

public sealed record CreateRequiredCutCommand(
    Guid ProjectSectionId,
    Guid MaterialId,
    string Name,
    int LengthMm,
    int Quantity
) : IRequest<Guid?>;