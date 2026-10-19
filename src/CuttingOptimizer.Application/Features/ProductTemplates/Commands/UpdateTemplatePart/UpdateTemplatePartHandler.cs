using CuttingOptimizer.Application.Abstractions;
using CuttingOptimizer.Application.Features.ProductTemplates;
using CuttingOptimizer.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.ProductTemplates.Commands.UpdateTemplatePart;

public sealed class UpdateTemplatePartHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateTemplatePartCommand, UpdateTemplatePartResult>
{
    public async Task<UpdateTemplatePartResult> Handle(
        UpdateTemplatePartCommand request,
        CancellationToken cancellationToken)
    {
        var part = await context.TemplateParts
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (part is null)
            return new UpdateTemplatePartNotFound();

        if (request.DefaultAluminumMaterialId is { } materialId)
        {
            var materialOk = await context.Materials.AnyAsync(
                m => m.Id == materialId
                     && m.IsActive
                     && m.Category == MaterialCategory.Aluminum,
                cancellationToken);

            if (!materialOk)
                return new UpdateTemplatePartInvalidMaterial();
        }

        part.DisplayName = request.DisplayName.Trim();
        part.QtyPerUnit = request.QtyPerUnit;
        part.WidthCoef = request.WidthCoef;
        part.HeightCoef = request.HeightCoef;
        part.OffsetMm = request.OffsetMm;
        part.UseSashWidthDivision = request.UseSashWidthDivision;
        part.DefaultAluminumMaterialId = request.DefaultAluminumMaterialId;

        await context.SaveChangesAsync(cancellationToken);

        return new UpdateTemplatePartSucceeded(ProductTemplateMapper.ToPartDto(part));
    }
}
