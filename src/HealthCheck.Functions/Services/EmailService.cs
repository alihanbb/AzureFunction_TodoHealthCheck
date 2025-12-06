using System.Net;
using System.Net.Mail;
using HealthCheck.Functions.Models;
using Microsoft.Extensions.Logging;

namespace HealthCheck.Functions.Services;

public interface IEmailService
{
    Task SendAlertAsync(List<HealthCheckResult> unhealthyResults);
}

public class GmailEmailService : IEmailService
{
    private readonly HealthCheckSettings _settings;
    private readonly ILogger<GmailEmailService> _logger;

    public GmailEmailService(HealthCheckSettings settings, ILogger<GmailEmailService> logger)
    {
        _settings = settings;
        _logger = logger;
    }

    public async Task SendAlertAsync(List<HealthCheckResult> unhealthyResults)
    {
        if (!unhealthyResults.Any())
        {
            _logger.LogInformation("No unhealthy services to report.");
            return;
        }

        if (string.IsNullOrEmpty(_settings.SmtpUsername) || string.IsNullOrEmpty(_settings.SmtpPassword))
        {
            _logger.LogWarning("SMTP credentials not configured. Skipping email notification.");
            _logger.LogWarning("Unhealthy services: {Services}", string.Join(", ", unhealthyResults.Select(r => r.ServiceName)));
            return;
        }

        try
        {
            using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(_settings.SmtpUsername, _settings.SmtpPassword)
            };

            var subject = $"🚨 Health Check Alert - {unhealthyResults.Count} Service(s) Unhealthy";
            var body = BuildEmailBody(unhealthyResults);

            var message = new MailMessage
            {
                From = new MailAddress(_settings.SmtpUsername),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            message.To.Add(_settings.NotificationEmail);

            await client.SendMailAsync(message);
            _logger.LogInformation("Alert email sent to {Email}", _settings.NotificationEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send alert email");
        }
    }

    private static string BuildEmailBody(List<HealthCheckResult> results)
    {
        var html = @"
<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Arial, sans-serif; }
        .alert { background-color: #f8d7da; padding: 15px; border-radius: 5px; margin-bottom: 20px; }
        table { border-collapse: collapse; width: 100%; }
        th, td { border: 1px solid #ddd; padding: 10px; text-align: left; }
        th { background-color: #dc3545; color: white; }
        .unhealthy { color: #dc3545; font-weight: bold; }
    </style>
</head>
<body>
    <div class='alert'>
        <h2>🚨 Health Check Alert</h2>
        <p>The following services are reporting unhealthy status:</p>
    </div>
    <table>
        <tr>
            <th>Service</th>
            <th>URL</th>
            <th>Status</th>
            <th>Error</th>
            <th>Checked At</th>
        </tr>";

        foreach (var result in results)
        {
            html += $@"
        <tr>
            <td>{result.ServiceName}</td>
            <td>{result.Url}</td>
            <td class='unhealthy'>{result.Status}</td>
            <td>{result.ErrorMessage ?? "-"}</td>
            <td>{result.CheckedAt:yyyy-MM-dd HH:mm:ss} UTC</td>
        </tr>";
        }

        html += @"
    </table>
    <p style='margin-top: 20px; color: #666;'>
        This is an automated message from the Health Check Monitoring System.
    </p>
</body>
</html>";

        return html;
    }
}
