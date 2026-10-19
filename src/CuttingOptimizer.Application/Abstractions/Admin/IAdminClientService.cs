using CuttingOptimizer.Application.Features.Admin;

namespace CuttingOptimizer.Application.Abstractions.Admin;

public interface IAdminClientService
{
    Task<IReadOnlyList<AdminClientListItemDto>> ListClientsAsync(
        CancellationToken cancellationToken);

    Task<AdminClientDetailDto?> GetClientAsync(
        Guid workshopId,
        CancellationToken cancellationToken);

    Task<CreateAdminClientResult?> CreateClientAsync(
        CreateAdminClientRequest request,
        bool arabic,
        CancellationToken cancellationToken);

    Task<AdminClientDetailDto?> UpdateClientAsync(
        Guid workshopId,
        UpdateAdminClientRequest request,
        CancellationToken cancellationToken);

    Task<AdminClientDetailDto?> UpsertSubscriptionAsync(
        Guid workshopId,
        UpsertSubscriptionRequest request,
        CancellationToken cancellationToken);

    Task<AdminClientDetailDto?> SuspendSubscriptionAsync(
        Guid workshopId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<AdminWorkshopUserDto>> ListUsersAsync(
        Guid workshopId,
        CancellationToken cancellationToken);

    Task<AddWorkshopUserResult?> AddUserAsync(
        Guid workshopId,
        AddWorkshopUserRequest request,
        bool arabic,
        CancellationToken cancellationToken);

    Task<AdminTempPasswordResult?> ResetUserPasswordAsync(
        Guid workshopId,
        string userId,
        string? newPassword,
        bool arabic,
        CancellationToken cancellationToken);

    Task<bool> SetUserLockoutAsync(
        Guid workshopId,
        string userId,
        bool locked,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<AdminPlanDto>> ListPlansAsync(
        CancellationToken cancellationToken);

    Task<AdminPlanDto?> UpdatePlanAsync(
        Guid planId,
        UpdateSubscriptionPlanRequest request,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<PublicPlanDto>> ListPublicPlansAsync(
        CancellationToken cancellationToken);

    Task<AdminReportSummaryDto> GetReportSummaryAsync(
        CancellationToken cancellationToken);
}
