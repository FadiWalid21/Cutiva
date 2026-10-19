using CuttingOptimizer.Application.Abstractions;
using CuttingOptimizer.Application.Features.ClientQuotes;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.ClientQuotes.Queries.GetClientQuote;

public sealed class GetClientQuoteHandler(
    IApplicationDbContext context)
    : IRequestHandler<GetClientQuoteQuery, ClientQuoteDto?>,
      IRequestHandler<GetClientQuoteByPlanQuery, ClientQuoteDto?>
{
    public async Task<ClientQuoteDto?> Handle(
        GetClientQuoteQuery request,
        CancellationToken cancellationToken) =>
        await ClientQuoteMapper.LoadDto(context, request.QuoteId, cancellationToken);

    public async Task<ClientQuoteDto?> Handle(
        GetClientQuoteByPlanQuery request,
        CancellationToken cancellationToken)
    {
        var id = await context.ClientQuotes
            .AsNoTracking()
            .Where(q =>
                q.ProjectId == request.ProjectId
                && q.CuttingPlanId == request.CuttingPlanId)
            .Select(q => (Guid?)q.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (id is null)
            return null;

        return await ClientQuoteMapper.LoadDto(context, id.Value, cancellationToken);
    }
}
