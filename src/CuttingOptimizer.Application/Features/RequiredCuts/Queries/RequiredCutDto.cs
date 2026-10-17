namespace CuttingOptimizer.Application.Features.RequiredCuts.Queries;


public sealed record RequiredCutDto(
    Guid Id,
    Guid ProjectSectionId,
    string ProjectSectionName,
    Guid MaterialId,
    string MaterialCode,
    string MaterialName,
    string Name,
    int LengthMm,
    int Quantity,
    long TotalLengthMm
);