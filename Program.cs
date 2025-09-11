using diji_card_alt.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Connection string override via environment variable already supported by default config layering.

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
      var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[]{"http://localhost:4200"};
      policy.WithOrigins(allowedOrigins)
          .WithMethods("GET","POST","PUT","DELETE","OPTIONS")
          .AllowAnyHeader()
          .WithExposedHeaders("Content-Disposition")
          .AllowCredentials();
    });
});

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(x =>
{
    x.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
           .ConfigureWarnings(w => w.Log(RelationalEventId.PendingModelChangesWarning));
});

// JWT Auth (basic secret for demo)
var jwtKey = builder.Configuration["Jwt:Key"] ?? "dev-secret-key-change-me-32chars"; // fallback dev key
var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(opt =>
{
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ClockSkew = TimeSpan.Zero
    };
});

// Authorization services (policy ileride eklenebilir)
builder.Services.AddAuthorization();

// Basic rate limiting (örnek): Özel link oluşturma endpoint'i için dakikada 5 istek
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("SpecialLinkCreate", httpContext => RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: httpContext.User?.Identity?.Name ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "anon",
        factory: key => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 2
        }));
    // Login brute force mitigation: 5 attempts / 1 minute per IP
    options.AddPolicy("Login", httpContext => RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anon",
        factory: key => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0
        }));
    // Token exchange (OAuthToken + Refresh) higher limit
    options.AddPolicy("TokenExchange", httpContext => RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anon",
        factory: key => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 20,
            Window = TimeSpan.FromMinutes(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 2
        }));
});

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Geliştirme ortamında HTTPS yönlendirmesini kapatıyoruz
// app.UseHttpsRedirection();

// Configure static files with cache control
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Disable caching for profile photos
        if (ctx.File.Name.StartsWith("profile-photos"))
        {
            ctx.Context.Response.Headers.Append("Cache-Control", "no-cache, no-store");
            ctx.Context.Response.Headers.Append("Pragma", "no-cache");
            ctx.Context.Response.Headers.Append("Expires", "-1");
        }
    }
});

// Global security headers
app.Use(async (context, next) =>
{
    context.Response.Headers.TryAdd("X-Content-Type-Options", "nosniff");
    context.Response.Headers.TryAdd("X-Frame-Options", "DENY");
    context.Response.Headers.TryAdd("Referrer-Policy", "no-referrer");
    context.Response.Headers.TryAdd("X-XSS-Protection", "0"); // modern browsers ignore / CSP recommended
    // Minimal CSP (can be tightened later)
    context.Response.Headers.TryAdd("Content-Security-Policy", "default-src 'self'; img-src 'self' data: blob:; style-src 'self' 'unsafe-inline'; script-src 'self'; object-src 'none'; base-uri 'self'; frame-ancestors 'none'");
    await next();
});

// Ensure profile-photos directory exists
var profilePhotosDir = Path.Combine(app.Environment.WebRootPath ?? "wwwroot", "profile-photos");
if (!Directory.Exists(profilePhotosDir))
{
    Directory.CreateDirectory(profilePhotosDir);
}
// Ensure a usable default.png exists; if missing OR suspiciously tiny, copy from internal seed if available
var defaultPngPath = Path.Combine(profilePhotosDir, "default.png");
var seedDefaultPath = Path.Combine(AppContext.BaseDirectory, "profile-photo-seed", "default.png");
try
{
    if (!File.Exists(defaultPngPath))
    {
        if (File.Exists(seedDefaultPath))
        {
            File.Copy(seedDefaultPath, defaultPngPath, true);
            Console.WriteLine("[Info] default.png seeded (was missing).");
        }
        else
        {
            Console.WriteLine("[Warn] default.png missing and no seed copy found.");
        }
    }
    else
    {
        var fi = new FileInfo(defaultPngPath);
        if (fi.Length < 1024 && File.Exists(seedDefaultPath))
        {
            File.Copy(seedDefaultPath, defaultPngPath, true);
            Console.WriteLine("[Info] default.png reseeded (previous file too small).");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"[Warn] default.png seed operation failed: {ex.Message}");
}

// Important: UseCors must come before UseAuthorization and MapControllers
app.UseCors("AllowAngularApp");
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapControllers();

// Auto apply EF Core migrations and seed a default test user if not exists
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();

        // Eğer veritabanı tamamen boşsa ilk demo kullanıcıyı ekle
        if (!await db.Users.AnyAsync())
        {
            db.Users.Add(new diji_card_alt.Models.User
            {
                UserId = "demo",
                FullName = "Demo Kullanıcı",
                Email = "demo@example.com",
                Password = "1234",
                JobTitle = "Tester",
                Company = "DemoCorp",
                IsPublic = true
            });
            await db.SaveChangesAsync();
            Console.WriteLine("[Seed] İlk demo kullanıcı eklendi (tamamen boş DB).");
        }
        
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Migration/Seeding error: {ex.Message}");
    }
}

app.Run();
