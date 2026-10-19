using CuttingOptimizer.Domain.Enums;

namespace CuttingOptimizer.Application.Features.Admin;

public sealed record AdminClientListItemDto(
    Guid WorkshopId,
    string WorkshopName,
    string? OwnerUserId,
    string? OwnerEmail,
    string? OwnerDisplayName,
    string? PlanCode,
    string? PlanName,
    int? MaxUsers,
    int SeatUsed,
    string? SubscriptionStatus,
    string? PaymentStatus,
    DateTime? EndsAtUtc,
    int? DaysRemaining,
    int ProjectCount);

public sealed record AdminClientDetailDto(
    Guid WorkshopId,
    string WorkshopName,
    decimal? ProductionPricePerM2,
    string? OwnerUserId,
    string? OwnerEmail,
    string? OwnerDisplayName,
    string? PlanCode,
    string? PlanName,
    int? MaxUsers,
    int SeatUsed,
    string? SubscriptionStatus,
    string? PaymentStatus,
    decimal? PaymentAmount,
    string? PaymentCurrency,
    string? PaymentNote,
    DateTime? PaymentConfirmedAtUtc,
    DateTime? StartsAtUtc,
    DateTime? EndsAtUtc,
    int? DaysRemaining,
    int ProjectCount,
    int QuoteCount,
    IReadOnlyList<AdminWorkshopUserDto> Users);

public sealed record AdminWorkshopUserDto(
    string UserId,
    string Email,
    string? DisplayName,
    bool IsPrimaryContact,
    IReadOnlyList<string> Roles,
    bool IsLockedOut);

public sealed record AdminPlanDto(
    Guid Id,
    string Code,
    string Name,
    decimal PriceSar,
    int DurationDays,
    int? MaxUsers,
    bool IsActive);

public sealed record UpdateSubscriptionPlanRequest(
    string Name,
    decimal PriceSar,
    int? MaxUsers,
    int? DurationDays,
    bool? IsActive);

public sealed record PublicPlanDto(
    string Code,
    string Name,
    decimal PriceSar,
    int DurationDays,
    int? MaxUsers);

public sealed record AdminReportSummaryDto(
    int TotalClients,
    int ActiveSubscriptions,
    int ExpiredSubscriptions,
    int SuspendedSubscriptions,
    int ExpiringIn7Days,
    IReadOnlyList<AdminPlanCountDto> ByPlan);

public sealed record AdminPlanCountDto(
    string PlanCode,
    string PlanName,
    int Count);

public sealed record CreateAdminClientRequest(
    string WorkshopName,
    string OwnerEmail,
    string? OwnerPassword,
    string? OwnerDisplayName,
    string PlanCode,
    int? DurationDays,
    PaymentStatus? PaymentStatus,
    decimal? PaymentAmount,
    string? PaymentCurrency,
    string? PaymentNote);

public sealed record UpdateAdminClientRequest(
    string WorkshopName,
    string? OwnerDisplayName);

public sealed record UpsertSubscriptionRequest(
    string PlanCode,
    int? DurationDays,
    DateTime? EndsAtUtc,
    PaymentStatus? PaymentStatus,
    decimal? PaymentAmount,
    string? PaymentCurrency,
    string? PaymentNote);

public sealed record AddWorkshopUserRequest(
    string Email,
    string? DisplayName,
    string? Password);

public sealed record AddWorkshopUserResult(
    AdminWorkshopUserDto User,
    string TemporaryPassword,
    bool EmailSent);

public sealed record CreateAdminClientResult(
    AdminClientDetailDto Client,
    string TemporaryPassword,
    bool EmailSent);

public sealed record AdminTempPasswordResult(
    string TemporaryPassword,
    bool EmailSent);

public sealed record ResetPasswordRequest(
    string? NewPassword);
