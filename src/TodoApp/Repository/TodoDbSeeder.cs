using Microsoft.EntityFrameworkCore;

namespace TodoApp.Repository;

public static class TodoDbSeeder
{
    public static async Task SeedAsync(TodoDbContext db)
    {
        // Check if data already exists
        if (await db.Todos.AnyAsync())
            return;

        var todos = new List<Todo>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Proje dokümantasyonunu hazırla",
                Description = "README.md ve API dökümantasyonunu tamamla",
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Birim testleri yaz",
                Description = "TodoApp için xUnit test projesi oluştur ve temel testleri yaz",
                IsCompleted = true,
                CreatedAt = DateTime.UtcNow.AddDays(-4),
                CompletedAt = DateTime.UtcNow.AddDays(-2)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Docker Compose kurulumu",
                Description = "SQL Server ve Seq için docker-compose.yml yapılandır",
                IsCompleted = true,
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                CompletedAt = DateTime.UtcNow.AddDays(-1)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Health Check endpoint'lerini test et",
                Description = "/health ve /health/ready endpoint'lerinin çalıştığını doğrula",
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Azure Functions entegrasyonu",
                Description = "HealthCheck.Functions projesini TodoApp ile entegre et",
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "CI/CD pipeline oluştur",
                Description = "GitHub Actions ile otomatik build ve deploy ayarla",
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Performans optimizasyonu",
                Description = "API response sürelerini analiz et ve iyileştirmeler yap",
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Logging yapılandırması",
                Description = "Serilog ve Seq entegrasyonunu tamamla",
                IsCompleted = true,
                CreatedAt = DateTime.UtcNow.AddDays(-6),
                CompletedAt = DateTime.UtcNow.AddDays(-5)
            }
        };

        await db.Todos.AddRangeAsync(todos);
        await db.SaveChangesAsync();

        Console.WriteLine($"✓ {todos.Count} adet todo başarıyla eklendi.");
    }
}
