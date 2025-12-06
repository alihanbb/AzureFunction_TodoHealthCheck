# TodoApp with Health Check Monitoring System

Bu proje, **Vertical Slice Architecture** ile yazılmış bir TodoApp API ve **Azure Functions Timer Trigger** ile health check monitoring sistemi içerir.

## 📁 Proje Yapısı

```
HealtCheck-AzureFunctions/
├── docker-compose.yml              # SQL Server & Seq yapılandırması
├── .dockerignore
├── .env.example
├── TodoHealthCheck.sln
│
├── src/
│   ├── TodoApp/                    # Vertical Slice Architecture API
│   │   ├── Features/
│   │   │   └── Todos/
│   │   │       ├── Create/         # POST /api/todos
│   │   │       ├── GetAll/         # GET /api/todos
│   │   │       ├── GetById/        # GET /api/todos/{id}
│   │   │       ├── Update/         # PUT /api/todos/{id}
│   │   │       ├── Delete/         # DELETE /api/todos/{id}
│   │   │       └── Complete/       # PATCH /api/todos/{id}/complete
│   │   │
│   │   ├── Repository/             # Data Access Layer
│   │   │   ├── Todo.cs             # Entity
│   │   │   ├── TodoDbContext.cs    # DbContext
│   │   │   ├── TodoDbSeeder.cs     # Seed Data
│   │   │   └── Migrations/         # EF Core Migrations
│   │   │       ├── *_InitialCreate.cs
│   │   │       └── TodoDbContextModelSnapshot.cs
│   │   │
│   │   ├── Properties/
│   │   │   └── launchSettings.json
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   ├── appsettings.Development.json
│   │   ├── appsettings.Docker.json
│   │   └── TodoApp.csproj
│   │
│   └── HealthCheck.Functions/      # Azure Functions
│       ├── Functions/
│       │   └── HealthCheckMonitorFunction.cs  # Timer Trigger (5 dk)
│       ├── Services/
│       │   ├── HealthCheckService.cs          # HTTP health check
│       │   └── EmailService.cs                # Outlook SMTP
│       ├── Models/
│       ├── Program.cs
│       ├── host.json
│       ├── local.settings.json
│       └── HealthCheck.Functions.csproj
```

## 🐳 Docker ile Başlatma

### 1. Docker Compose Çalıştırma

```powershell
# SQL Server ve Seq container'ları başlat
docker compose up -d
```

Bu komut şu servisleri başlatır:
- **SQL Server** (port 1436) - Veritabanı
- **Seq** (port 8082) - Log yönetim arayüzü

### 2. Servislerin Durumunu Kontrol Et

```powershell
docker ps
```

## 🚀 Başlangıç

### 1. TodoApp'i Çalıştırma

```powershell
cd src/TodoApp
dotnet run
```

API şu adresten erişilebilir: `http://localhost:5156`

### 2. Swagger UI

Swagger dokümantasyonu için: `http://localhost:5156/swagger`

### 3. Seq Log Arayüzü

Log'ları görüntülemek için: `http://localhost:8082`

## 🗄️ Veritabanı Yönetimi

### Migration Oluşturma

```powershell
cd src/TodoApp

# Yeni migration oluştur (Repository/Migrations klasörüne)
dotnet ef migrations add MigrationName --output-dir Repository/Migrations

# Veritabanını güncelle
dotnet ef database update
```

### Migration Geri Alma

```powershell
# Son migration'ı geri al
dotnet ef migrations remove
```

### Seed Data

Uygulama ilk çalıştığında otomatik olarak 8 adet örnek Todo verisi oluşturur:
- 3 tamamlanmış todo
- 5 bekleyen todo

## 📡 Health Check Endpoints

| Endpoint | Açıklama |
|----------|----------|
| `/health` | Genel health durumu (DB dahil) |
| `/health/ready` | Readiness probe |
| `/health/live` | Liveness probe |

## 📡 API Endpoints

### Todos CRUD

| Method | Endpoint | Açıklama |
|--------|----------|----------|
| `POST` | `/api/todos` | Yeni todo oluştur |
| `GET` | `/api/todos` | Tüm todoları listele |
| `GET` | `/api/todos/{id}` | Tek todo getir |
| `PUT` | `/api/todos/{id}` | Todo güncelle |
| `DELETE` | `/api/todos/{id}` | Todo sil |
| `PATCH` | `/api/todos/{id}/complete` | Tamamlandı olarak işaretle |

## 🔧 Azure Functions'ı Çalıştırma

```powershell
# local.settings.json dosyasını oluştur
cp src/HealthCheck.Functions/local.settings.sample.json src/HealthCheck.Functions/local.settings.json

# Email ayarlarını düzenle
# Sonra çalıştır:
cd src/HealthCheck.Functions
func start
```

## ⚙️ Yapılandırma

### Email Ayarları (local.settings.json)

```json
{
  "Values": {
    "HealthCheck:ServiceUrls": "http://localhost:5156/health;http://localhost:5156/health/ready",
    "Email:SmtpHost": "smtp.office365.com",
    "Email:SmtpPort": "587",
    "Email:SmtpUsername": "your-email@outlook.com",
    "Email:SmtpPassword": "your-app-password",
    "Email:NotificationEmail": "alert-receiver@example.com"
  }
}
```

> **Not**: Outlook'ta 2FA aktifse, [App Password](https://account.microsoft.com/security) oluşturmanız gerekir.

## 🔔 Bildirim Sistemi

Timer Trigger her **5 dakikada bir** çalışır:

1. Yapılandırılan health endpoint'lerini kontrol eder
2. Unhealthy servisler tespit edilirse email gönderir
3. Tüm sonuçları Seq'e loglar

## 🛠️ Teknolojiler

- **.NET 9.0** - TodoApp API
- **.NET 8.0** - Azure Functions
- **MediatR** - CQRS pattern
- **EF Core SQL Server** - Veritabanı
- **Serilog + Seq** - Yapılandırılmış logging
- **Docker Compose** - Container orchestration
- **Azure Functions Timer Trigger** - Periyodik monitoring
- **SMTP** - Outlook email bildirim

## 📊 Erişim Noktaları

| Servis | URL | Açıklama |
|--------|-----|----------|
| TodoApp API | http://localhost:5156 | API Endpoints |
| Swagger UI | http://localhost:5156/swagger | API Dokümantasyonu |
| Health Check | http://localhost:5156/health | Sağlık Durumu |
| Seq | http://localhost:8082 | Log Arayüzü |
| SQL Server | localhost:1436 | Veritabanı |
