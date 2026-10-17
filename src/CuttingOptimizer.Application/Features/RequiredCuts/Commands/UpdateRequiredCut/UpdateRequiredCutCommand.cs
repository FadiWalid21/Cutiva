using MediatR;

namespace CuttingOptimizer.Application.Features.RequiredCuts.Commands.UpdateRequiredCut;

public sealed record UpdateRequiredCutCommand(
    Guid Id,
    Guid ProjectSectionId,
    Guid MaterialId,
    string Name,
    int LengthMm,
    int Quantity
) : IRequest<bool>;