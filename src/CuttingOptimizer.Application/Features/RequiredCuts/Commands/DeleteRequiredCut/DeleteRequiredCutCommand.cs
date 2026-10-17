using MediatR;

namespace CuttingOptimizer.Application.Features.RequiredCuts.Commands.DeleteRequiredCut;

public sealed record DeleteRequiredCutCommand(Guid Id) : IRequest<bool>;
