namespace CuttingOptimizer.Infrastructure.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Key { get; set; } = string.Empty;

    public string Issuer { get; set; } = "Cutiva";

    public string Audience { get; set; } = "Cutiva";

    public int ExpiresMinutes { get; set; } = 10080;
}
