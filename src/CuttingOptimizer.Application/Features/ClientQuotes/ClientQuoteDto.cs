using CuttingOptimizer.Application.Features.Accessories;

namespace CuttingOptimizer.Application.Features.ClientQuotes;

public sealed record ClientQuoteDto(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    string? CustomerName,
    Guid? CuttingPlanId,
    Guid WorkshopId,
    string WorkshopName,
    string MaterialCode,
    string MaterialName,
    decimal MaterialCost,
    decimal ProductionAreaM2,
    decimal? ProductionPricePerM2,
    decimal ProductionCost,
    decimal AccessoriesTotal,
    decimal GlassTotal,
    /// <summary>Aluminum cost from product-item calculation snapshots (0 if none).</summary>
    decimal ProductAluminumTotal,
    decimal SurchargesTotal,
    decimal GrandTotal,
    string? Notes,
    DateTime CreatedAtUtc,
    IReadOnlyList<ClientQuoteSurchargeDto> Surcharges,
    IReadOnlyList<QuoteAccessoryLineDto> Accessories,
    IReadOnlyList<QuoteGlassLineDto> GlassLines,
    IReadOnlyList<QuoteSectionPreviewDto> Sections);

public sealed record QuoteGlassLineDto(
    Guid GlassSpecId,
    string Name,
    int WidthMm,
    int HeightMm,
    decimal AreaM2PerPane,
    int PaneQuantity,
    decimal PricePerSqm,
    decimal LineTotal);

public sealed record ClientQuoteSurchargeDto(
    Guid Id,
    string Reason,
    decimal Amount);

public sealed record QuoteSectionPreviewDto(
    Guid SectionId,
    string Name,
    string SectorCode,
    string SectorName,
    int? HeightMm,
    int? WidthMm,
    int? UnitQuantity,
    bool CountsForProduction);

public sealed record ClientQuoteSurchargeInput(
    string Reason,
    decimal Amount);

public sealed record ClientQuoteSummaryDto(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    string? CustomerName,
    Guid? CuttingPlanId,
    string MaterialName,
    decimal GrandTotal,
    DateTime CreatedAtUtc);
