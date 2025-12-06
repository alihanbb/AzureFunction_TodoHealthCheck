using HealthCheck.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace HealthCheck.Functions.Functions;

public class HealthCheckMonitorFunction
{
    private readonly IHealthCheckService _healthCheckService;
    private readonly IEmailService _emailService;
    private readonly ILogger<HealthCheckMonitorFunction> _logger;

    public HealthCheckMonitorFunction(
        IHealthCheckService healthCheckService,
        IEmailService emailService,
        ILogger<HealthCheckMonitorFunction> logger)
    {
        _healthCheckService = healthCheckService;
        _emailService = emailService;
        _logger = logger;
    }

    [Function("HealthCheckMonitor")]
    public async Task Run([TimerTrigger("*/30 * * * * *")] TimerInfo timerInfo)
    {
        var executionId = Guid.NewGuid().ToString("N")[..8];
        
        _logger.LogInformation(
            "========== HEALTH CHECK STARTED ==========\n" +
            "ExecutionId: {ExecutionId}\n" +
            "Timestamp: {Timestamp:yyyy-MM-dd HH:mm:ss} UTC",
            executionId, DateTime.UtcNow);

        try
        {
            var results = await _healthCheckService.CheckAllServicesAsync();

            var healthy = results.Where(r => r.Status == "Healthy").ToList();
            var unhealthy = results.Where(r => r.Status != "Healthy").ToList();

            // Summary log
            _logger.LogInformation(
                "---------- CHECK SUMMARY ----------\n" +
                "Total Services: {TotalCount}\n" +
                "Healthy: {HealthyCount} | Unhealthy: {UnhealthyCount}",
                results.Count, healthy.Count, unhealthy.Count);

            // Detail logs for each service
            foreach (var result in results)
            {
                var statusIcon = result.Status == "Healthy" ? "[OK]" : "[FAIL]";
                var logLevel = result.Status == "Healthy" ? LogLevel.Information : LogLevel.Warning;
                
                _logger.Log(logLevel,
                    "{StatusIcon} {ServiceName}\n" +
                    "   URL: {Url}\n" +
                    "   Status: {Status} (HTTP {StatusCode})\n" +
                    "   Response Time: {ResponseTime}ms\n" +
                    "   Checked At: {CheckedAt:HH:mm:ss}",
                    statusIcon, result.ServiceName, result.Url, 
                    result.Status, result.StatusCode, 
                    result.ResponseTimeMs, result.CheckedAt);

                if (!string.IsNullOrEmpty(result.ErrorMessage))
                {
                    _logger.LogWarning("   Error: {ErrorMessage}", result.ErrorMessage);
                }
            }

            // Send alert if any unhealthy
            if (unhealthy.Any())
            {
                _logger.LogWarning(
                    "---------- ALERT: UNHEALTHY SERVICES ----------\n" +
                    "Services: {UnhealthyServices}",
                    string.Join(", ", unhealthy.Select(u => u.ServiceName)));

                await _emailService.SendAlertAsync(unhealthy);
                _logger.LogInformation("Alert email sent successfully");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "---------- HEALTH CHECK FAILED ----------\n" +
                "ExecutionId: {ExecutionId}\n" +
                "Error: {ErrorMessage}", 
                executionId, ex.Message);
            throw;
        }

        _logger.LogInformation(
            "========== HEALTH CHECK COMPLETED ==========\n" +
            "ExecutionId: {ExecutionId}\n" +
            "Next Check: {NextRun:yyyy-MM-dd HH:mm:ss} UTC",
            executionId, timerInfo.ScheduleStatus?.Next);
    }
}
