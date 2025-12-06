namespace HealthCheck.Functions.Models;

public class HealthCheckResult
{
    public string ServiceName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CheckedAt { get; set; }
    public long ResponseTimeMs { get; set; }
}

public class HealthCheckSettings
{
    public string[] ServiceUrls { get; set; } = Array.Empty<string>();
    public string SmtpHost { get; set; } = "smtp.gmail.com";
    public int SmtpPort { get; set; } = 587;
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public string NotificationEmail { get; set; } = string.Empty;
}
