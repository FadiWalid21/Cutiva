using CuttingOptimizer.Application.Abstractions.Auth;
using CuttingOptimizer.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Infrastructure.Auth;

public sealed class IdentityAuthService(
    UserManager<ApplicationUser> userManager,
    JwtTokenService jwtTokenService)
    : IAuthService
{
    public async Task<AuthResult> RegisterAsync(
        string email,
        string password,
        string? displayName,
        CancellationToken cancellationToken)
    {
        var existing = await userManager.FindByEmailAsync(email);
        if (existing is not null)
            throw new AuthDuplicateEmailException();

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            DisplayName = displayName,
            EmailConfirmed = true
        };

        var create = await userManager.CreateAsync(user, password);
        if (!create.Succeeded)
        {
            throw new AuthOperationException(
                create.Errors.Select(error => error.Description).ToList());
        }

        var (token, expires) = jwtTokenService.CreateToken(user);
        return new AuthResult(
            token,
            expires,
            user.Email!,
            user.DisplayName,
            user.Id);
    }

    public async Task<AuthResult?> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
            return null;

        var valid = await userManager.CheckPasswordAsync(user, password);
        if (!valid)
            return null;

        var (token, expires) = jwtTokenService.CreateToken(user);
        return new AuthResult(
            token,
            expires,
            user.Email!,
            user.DisplayName,
            user.Id);
    }

    public async Task<AuthUserDto?> GetMeAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        var user = await userManager.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
            return null;

        return new AuthUserDto(user.Id, user.Email!, user.DisplayName);
    }
}
