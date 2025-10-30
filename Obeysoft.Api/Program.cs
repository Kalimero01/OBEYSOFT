using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Obeysoft.Api.Middlewares;
using Obeysoft.Application.Comments;
using Obeysoft.Infrastructure;
using Obeysoft.Infrastructure.Persistence;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------
// 1) PORT AYARI
// Render üretimde PORT env veriyor (mesela 10000).
// Lokal geliştirirken 5052 kullanmak istiyoruz.
// ---------------------------------------------------------------------
var portFromEnv = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(portFromEnv))
{
    // Render / container yolu → 0.0.0.0:PORT
    builder.WebHost.UseUrls($"http://0.0.0.0:{portFromEnv}");
}
else
{
    // Lokal yol → hep 5052
    builder.WebHost.UseUrls("http://localhost:5052");
}

// -------- Services --------
var configuration = builder.Configuration;

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger + JWT şeması
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Obeysoft API", Version = "v1" });

    var jwtScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "JWT Bearer token. Örnek: Bearer {token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };

    c.AddSecurityDefinition("Bearer", jwtScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtScheme, Array.Empty<string>() }
    });
});

// Infrastructure (DbContext + servisler)
builder.Services.AddInfrastructure(configuration);

// FluentValidation — CreateComment için kayıt
builder.Services.AddScoped<IValidator<CreateCommentRequestDto>, CreateCommentRequestDtoValidator>();

// Global Exception Middleware kaydı
builder.Services.AddTransient<ExceptionHandlingMiddleware>();

// CORS
var corsOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:5173" };
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("Default", policy =>
    {
        policy.WithOrigins(corsOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// JWT
var jwtSection = configuration.GetSection("Jwt");
var jwtKey = jwtSection.GetValue<string>("Key") ?? throw new InvalidOperationException("Jwt:Key eksik.");
var jwtIssuer = jwtSection.GetValue<string>("Issuer") ?? "Obeysoft";
var jwtAudience = jwtSection.GetValue<string>("Audience") ?? "ObeysoftClient";
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        // Render HTTPS sonlandırmayı kendisi yapıyor, biz HTTP dinliyoruz.
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// -------- Pipeline --------
app.UseSwagger();
app.UseSwaggerUI();

// Global hata yakalama
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseCors("Default");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ---- Otomatik migrate + opsiyonel seed ----
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BlogDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");

    try
    {
        var automigrate = configuration.GetSection("Database").GetValue<bool>("Automigrate");
        if (automigrate)
        {
            await db.Database.EnsureCreatedAsync();
            await db.Database.MigrateAsync();
            logger.LogInformation("✅ Database migrated.");
        }

        var seed = configuration.GetSection("Database").GetValue<bool>("Seed");
        if (seed)
        {
            await BlogDbSeeder.SeedAsync(db, logger);
        }
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "DB migrate/seed sırasında hata; API ayakta kalmaya devam ediyor.");
    }
}

await app.RunAsync();
