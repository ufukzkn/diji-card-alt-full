using diji_card_alt.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .WithExposedHeaders("Content-Disposition") // Needed for file downloads
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
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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

// Ensure profile-photos directory exists
var profilePhotosDir = Path.Combine(app.Environment.WebRootPath ?? "wwwroot", "profile-photos");
if (!Directory.Exists(profilePhotosDir))
{
    Directory.CreateDirectory(profilePhotosDir);
}

// Important: UseCors must come before UseAuthorization and MapControllers
app.UseCors("AllowAngularApp");
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapControllers();

app.Run();
