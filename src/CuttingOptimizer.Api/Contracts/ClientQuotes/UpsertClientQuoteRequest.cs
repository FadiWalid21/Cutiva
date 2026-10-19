namespace CuttingOptimizer.Api.Contracts.ClientQuotes;

public sealed class UpsertClientQuoteRequest
{
    public Guid ProjectId { get; init; }

    /// <summary>Null for glass/accessories-only quotes.</summary>
    public Guid? CuttingPlanId { get; init; }

    public string? Notes { get; init; }

    public IReadOnlyList<ClientQuoteSurchargeRequest>? Surcharges { get; init; }
}

public sealed class ClientQuoteSurchargeRequest
{
    public string Reason { get; init; } = string.Empty;

    public decimal Amount { get; init; }
}
