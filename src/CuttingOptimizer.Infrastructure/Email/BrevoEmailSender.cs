using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using CuttingOptimizer.Application.Abstractions.Email;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CuttingOptimizer.Infrastructure.Email;

public sealed class BrevoEmailSender(
    HttpClient httpClient,
    IOptions<BrevoOptions> options,
    ILogger<BrevoEmailSender> logger)
    : IEmailSender
{
    public async Task SendAsync(
        EmailMessage message,
        CancellationToken cancellationToken = default)
    {
        var opts = options.Value;
        var missing = new List<string>();
        if (string.IsNullOrWhiteSpace(opts.ApiKey))
            missing.Add("Brevo:ApiKey");
        if (string.IsNullOrWhiteSpace(opts.SenderEmail))
            missing.Add("Brevo:SenderEmail");

        if (missing.Count > 0)
        {
            logger.LogWarning(
                "Brevo is not configured (missing {Missing}); skipping email to {Email} ({Subject})",
                string.Join(", ", missing),
                message.ToEmail,
                message.Subject);
            throw new EmailSendException(
                $"Brevo is not configured (missing {string.Join(", ", missing)}).");
        }

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "https://api.brevo.com/v3/smtp/email");
        request.Headers.Add("api-key", opts.ApiKey);
        request.Headers.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        request.Content = JsonContent.Create(new BrevoSendRequest(
            new BrevoSender(opts.SenderEmail, opts.SenderName),
            [new BrevoRecipient(message.ToEmail, message.ToName)],
            message.Subject,
            message.HtmlBody));

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogError(
                "Brevo send failed ({Status}): {Body}",
                (int)response.StatusCode,
                body);
            throw new EmailSendException(
                $"Brevo send failed with status {(int)response.StatusCode}.");
        }
    }

    private sealed record BrevoSendRequest(
        [property: JsonPropertyName("sender")] BrevoSender Sender,
        [property: JsonPropertyName("to")] IReadOnlyList<BrevoRecipient> To,
        [property: JsonPropertyName("subject")] string Subject,
        [property: JsonPropertyName("htmlContent")] string HtmlContent);

    private sealed record BrevoSender(
        [property: JsonPropertyName("email")] string Email,
        [property: JsonPropertyName("name")] string Name);

    private sealed record BrevoRecipient(
        [property: JsonPropertyName("email")] string Email,
        [property: JsonPropertyName("name")] string? Name);
}
