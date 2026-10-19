using CuttingOptimizer.Application.Features.ClientQuotes;
using MediatR;

namespace CuttingOptimizer.Application.Features.ClientQuotes.Queries.GetClientQuote;

public sealed record GetClientQuoteQuery(Guid QuoteId)
    : IRequest<ClientQuoteDto?>;

public sealed record GetClientQuoteByPlanQuery(
    Guid ProjectId,
    Guid CuttingPlanId)
    : IRequest<ClientQuoteDto?>;
