using CuttingOptimizer.Application.Features.ClientQuotes;
using MediatR;

namespace CuttingOptimizer.Application.Features.ClientQuotes.Commands.UpsertClientQuote;

public sealed record UpsertClientQuoteCommand(
    Guid ProjectId,
    Guid? CuttingPlanId,
    string? Notes,
    IReadOnlyList<ClientQuoteSurchargeInput>? Surcharges)
    : IRequest<UpsertClientQuoteResult>;

public abstract record UpsertClientQuoteResult;

public sealed record UpsertClientQuoteSucceeded(ClientQuoteDto Quote)
    : UpsertClientQuoteResult;

public sealed record UpsertClientQuoteNotFound : UpsertClientQuoteResult;

public sealed record UpsertClientQuotePlanNotApproved : UpsertClientQuoteResult;

public sealed record UpsertClientQuoteNoBillableLines : UpsertClientQuoteResult;

public sealed record UpsertClientQuoteFeatureDisabled : UpsertClientQuoteResult;
