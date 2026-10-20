using CuttingOptimizer.Application.Abstractions;
using CuttingOptimizer.Application.Abstractions.Entitlements;
using CuttingOptimizer.Application.Features.ClientQuotes;
using CuttingOptimizer.Domain.Constants;
using CuttingOptimizer.Domain.Entities;
using CuttingOptimizer.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.ClientQuotes.Commands.UpsertClientQuote;

public sealed class UpsertClientQuoteHandler(
    IApplicationDbContext context,
    IPlanEntitlementService entitlements)
    : IRequestHandler<UpsertClientQuoteCommand, UpsertClientQuoteResult>
{
    public async Task<UpsertClientQuoteResult> Handle(
        UpsertClientQuoteCommand request,
        CancellationToken cancellationToken)
    {
        var project = await context.Projects
            .AsNoTracking()
            .Include(p => p.Workshop)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);

        if (project is null)
            return new UpsertClientQuoteNotFound();

        if (!await entitlements.HasAsync(
                project.WorkshopId,
                PlanFeatureCodes.ClientQuotes,
                cancellationToken))
            return new UpsertClientQuoteFeatureDisabled();

        if (request.CuttingPlanId.HasValue)
        {
            var plan = await context.CuttingPlans
                .FirstOrDefaultAsync(
                    p => p.Id == request.CuttingPlanId.Value
                         && p.ProjectId == request.ProjectId,
                    cancellationToken);

            if (plan is null)
                return new UpsertClientQuoteNotFound();

            if (plan.Status != CuttingPlanStatus.Approved)
                return new UpsertClientQuotePlanNotApproved();
        }
        else
        {
            // Quote without a cutting plan is allowed only when accessories exist.
            var hasAccessories = await context.ProjectSectionAccessories
                .AnyAsync(
                    a => a.ProjectSection.ProjectId == request.ProjectId,
                    cancellationToken);

            if (!hasAccessories)
                return new UpsertClientQuoteNoBillableLines();
        }

        ClientQuote? quote;
        if (request.CuttingPlanId.HasValue)
        {
            quote = await context.ClientQuotes
                .Include(q => q.Surcharges)
                .FirstOrDefaultAsync(
                    q => q.ProjectId == request.ProjectId
                         && q.CuttingPlanId == request.CuttingPlanId,
                    cancellationToken);
        }
        else
        {
            quote = await context.ClientQuotes
                .Include(q => q.Surcharges)
                .FirstOrDefaultAsync(
                    q => q.ProjectId == request.ProjectId
                         && q.CuttingPlanId == null,
                    cancellationToken);
        }

        if (quote is null)
        {
            quote = new ClientQuote
            {
                ProjectId = request.ProjectId,
                CuttingPlanId = request.CuttingPlanId,
                CreatedAtUtc = DateTime.UtcNow,
                Notes = TrimNotes(request.Notes)
            };
            context.ClientQuotes.Add(quote);
        }
        else
        {
            quote.Notes = TrimNotes(request.Notes);
            context.ClientQuoteSurcharges.RemoveRange(quote.Surcharges);
            quote.Surcharges.Clear();
        }

        if (request.Surcharges is not null)
        {
            foreach (var line in request.Surcharges)
            {
                var reason = line.Reason?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(reason) || line.Amount == 0)
                    continue;

                quote.Surcharges.Add(new ClientQuoteSurcharge
                {
                    Reason = reason.Length > 300 ? reason[..300] : reason,
                    Amount = line.Amount
                });
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        var dto = await ClientQuoteMapper.LoadDto(
            context,
            quote.Id,
            cancellationToken);

        return dto is null
            ? new UpsertClientQuoteNotFound()
            : new UpsertClientQuoteSucceeded(dto);
    }

    private static string? TrimNotes(string? notes)
    {
        if (string.IsNullOrWhiteSpace(notes))
            return null;
        var trimmed = notes.Trim();
        return trimmed.Length > 1000 ? trimmed[..1000] : trimmed;
    }
}
