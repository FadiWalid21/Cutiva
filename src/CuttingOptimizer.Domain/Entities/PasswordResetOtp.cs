namespace CuttingOptimizer.Domain.Entities;

public class PasswordResetOtp
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string UserId { get; set; } = string.Empty;

    public string CodeHash { get; set; } = string.Empty;

    public DateTime ExpiresAtUtc { get; set; }

    public DateTime? ConsumedAtUtc { get; set; }

    public int AttemptCount { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
