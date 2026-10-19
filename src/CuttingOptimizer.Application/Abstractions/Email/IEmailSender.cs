namespace CuttingOptimizer.Application.Abstractions.Email;

public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}

public sealed record EmailMessage(
    string ToEmail,
    string? ToName,
    string Subject,
    string HtmlBody);

public sealed class EmailSendException : Exception
{
    public EmailSendException(string message, Exception? inner = null)
        : base(message, inner)
    {
    }
}
