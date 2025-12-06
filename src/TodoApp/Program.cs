using Microsoft.EntityFrameworkCore;
using Serilog;
using TodoApp.Features.Todos.Create;
using TodoApp.Features.Todos.GetAll;
using TodoApp.Features.Todos.GetById;
using TodoApp.Features.Todos.Update;
using TodoApp.Features.Todos.Delete;
using TodoApp.Features.Todos.Complete;
using TodoApp.Repository;

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .Build())
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Serilog
    builder.Host.UseSerilog((context, loggerConfig) =>
        loggerConfig.ReadFrom.Configuration(context.Configuration));

    // Swagger
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "TodoApp API", Version = "v1" });
    });

    // DbContext
    builder.Services.AddDbContext<TodoDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    // Health checks
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<TodoDbContext>("database");

    // MediatR
    builder.Services.AddMediatR(cfg =>
        cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

    var app = builder.Build();

    // HTTP request logging
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
        };
    });

    // Swagger
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TodoApp API V1");
    });

    // Health check endpoints
    app.MapHealthChecks("/health");
    app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = _ => true
    });
    app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = _ => false
    });

    // API Endpoints
    CreateTodoEndpoint.Map(app);
    GetAllTodosEndpoint.Map(app);
    GetTodoByIdEndpoint.Map(app);
    UpdateTodoEndpoint.Map(app);
    DeleteTodoEndpoint.Map(app);
    CompleteTodoEndpoint.Map(app);

    // Database initialization and seeding
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
        
        // Apply pending migrations
        await db.Database.MigrateAsync();
        
        // Seed data
        await TodoDbSeeder.SeedAsync(db);
    }

    Log.Information("TodoApp basariyla baslatildi");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "TodoApp baslatilirken hata olustu");
}
finally
{
    Log.CloseAndFlush();
}
