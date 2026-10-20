namespace CuttingOptimizer.Api.Cors;

public static class CorsOriginPolicy
{
    public const string PolicyName = "Frontend";

    public static bool IsAllowed(string? origin, string[] configured)
    {
        if (string.IsNullOrWhiteSpace(origin))
            return false;

        if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
            return false;

        if (configured.Any(o =>
                string.Equals(o, origin, StringComparison.OrdinalIgnoreCase)))
            return true;

        // Local Angular dev
        if (uri.IsLoopback && (uri.Port == 4200 || uri.Port == 4201))
            return true;

        // Vercel production + preview deployments (https://*.vercel.app)
        if (uri.Scheme == Uri.UriSchemeHttps
            && uri.Host.EndsWith(".vercel.app", StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }
}
