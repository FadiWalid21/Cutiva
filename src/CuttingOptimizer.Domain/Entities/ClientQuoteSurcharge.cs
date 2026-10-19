namespace CuttingOptimizer.Domain.Entities;

public class ClientQuoteSurcharge
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ClientQuoteId { get; set; }

    public string Reason { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public ClientQuote ClientQuote { get; set; } = null!;
}
