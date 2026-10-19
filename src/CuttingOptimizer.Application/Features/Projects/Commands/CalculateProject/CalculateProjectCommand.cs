using MediatR;

namespace CuttingOptimizer.Application.Features.Projects.Commands.CalculateProject;

public sealed record CalculateProjectCommand(
    Guid ProjectId,
    int KerfMm = 3,
    bool UseRemnants = true)
    : IRequest<CalculateProjectCommandResult>;

public abstract record CalculateProjectCommandResult;

public sealed record CalculateProjectSucceeded(CalculateProjectResultDto Result)
    : CalculateProjectCommandResult;

public sealed record CalculateProjectNotFound : CalculateProjectCommandResult;

public sealed record CalculateProjectNoItems : CalculateProjectCommandResult;

public sealed record CalculateProjectFailed(string Code, string Message)
    : CalculateProjectCommandResult;
