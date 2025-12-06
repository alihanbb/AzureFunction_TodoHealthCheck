using HealthCheck.Functions.Models;
using HealthCheck.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

var settings = new HealthCheckSettings
{
    ServiceUrls = (Environment.GetEnvironmentVariable("HealthCheck:ServiceUrls") ?? "http://localhost:5000/health")
        .Split(';', StringSplitOptions.RemoveEmptyEntries),
    SmtpHost = Environment.GetEnvironmentVariable("Email:SmtpHost") ?? "smtp.gmail.com",
    SmtpPort = int.TryParse(Environment.GetEnvironmentVariable("Email:SmtpPort"), out var port) ? port : 587,
    SmtpUsername = Environment.GetEnvironmentVariable("Email:SmtpUsername") ?? "",
    SmtpPassword = Environment.GetEnvironmentVariable("Email:SmtpPassword") ?? "",
    NotificationEmail = Environment.GetEnvironmentVariable("Email:NotificationEmail") ?? ""
};

builder.Services.AddSingleton(settings);
builder.Services.AddHttpClient<IHealthCheckService, HealthCheckService>();
builder.Services.AddSingleton<IEmailService, GmailEmailService>();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Build().Run();
