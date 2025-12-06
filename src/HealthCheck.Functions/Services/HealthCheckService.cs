using System.Diagnostics;
using HealthCheck.Functions.Models;
using Microsoft.Extensions.Logging;

namespace HealthCheck.Functions.Services;

public interface IHealthCheckService
{
    Task<List<HealthCheckResult>> CheckAllServicesAsync();
}

public class HealthCheckService : IHealthCheckService
{
    private readonly HttpClient _httpClient;
    private readonly HealthCheckSettings _settings;
    private readonly ILogger<HealthCheckService> _logger;

    public HealthCheckService(HttpClient httpClient, HealthCheckSettings settings, ILogger<HealthCheckService> logger)
    {
        _httpClient = httpClient;
        _settings = settings;
        _logger = logger;
    }

    public async Task<List<HealthCheckResult>> CheckAllServicesAsync()
    {
        var results = new List<HealthCheckResult>();

        foreach (var url in _settings.ServiceUrls)
        {
            var result = await CheckServiceAsync(url);
            results.Add(result);
        }

        return results;
    }

    private async Task<HealthCheckResult> CheckServiceAsync(string url)
    {
        var result = new HealthCheckResult
        {
            ServiceName = ExtractServiceName(url),
            Url = url,
            CheckedAt = DateTime.UtcNow
        };

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await _httpClient.GetAsync(url);
            stopwatch.Stop();

            result.StatusCode = (int)response.StatusCode;
            result.Status = response.IsSuccessStatusCode ? "Healthy" : "Unhealthy";
            result.ResponseTimeMs = stopwatch.ElapsedMilliseconds;

            if (!response.IsSuccessStatusCode)
            {
                result.ErrorMessage = $"HTTP {result.StatusCode}: {response.ReasonPhrase}";
            }

            _logger.LogInformation("Health check for {Url}: {Status} ({ResponseTime}ms)", url, result.Status, result.ResponseTimeMs);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            result.Status = "Unhealthy";
            result.StatusCode = 0;
            result.ErrorMessage = ex.Message;
            result.ResponseTimeMs = stopwatch.ElapsedMilliseconds;

            _logger.LogError(ex, "Health check failed for {Url}", url);
        }

        return result;
    }

    private static string ExtractServiceName(string url)
    {
        try
        {
            var uri = new Uri(url);
            return uri.Host.Replace("localhost", "TodoApp");
        }
        catch
        {
            return url;
        }
    }
}
