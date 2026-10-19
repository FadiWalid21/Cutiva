using CuttingOptimizer.Application.Features.ClientQuotes;
using MediatR;

namespace CuttingOptimizer.Application.Features.ClientQuotes.Queries.ListWorkshopQuotes;

public sealed record ListWorkshopQuotesQuery(Guid WorkshopId)
    : IRequest<IReadOnlyList<ClientQuoteSummaryDto>?>;
