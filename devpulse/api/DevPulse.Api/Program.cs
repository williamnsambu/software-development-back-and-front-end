using System;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Quartz;
using DevPulse.Infrastructure;
using DevPulse.Application;
using DevPulse.Infrastructure.Jobs;

var builder = WebApplication.CreateBuilder(args);
var cfg = builder.Configuration;

// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "DevPulse API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Paste your JWT access token here (no need to type 'Bearer ')."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Data Protection
builder.Services.AddDataProtection();

// JWT auth (supports Base64 key first, falls back to raw string)
static byte[] GetSigningKeyBytes(IConfiguration c)
{
    var b64 = c["Auth:SigningKeyBase64"];
    if (!string.IsNullOrWhiteSpace(b64))
        return Convert.FromBase64String(b64);

    var raw = c["Auth:SigningKey"]
              ?? throw new InvalidOperationException("Missing Auth:SigningKey or Auth:SigningKeyBase64.");
    var bytes = Encoding.UTF8.GetBytes(raw);
    if (bytes.Length < 32) throw new InvalidOperationException("Signing key must be at least 32 bytes.");
    return bytes;
}

builder.Services.AddAuthentication(o =>
{
    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = cfg["Auth:Issuer"],
        ValidAudience = cfg["Auth:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(GetSigningKeyBytes(cfg))
    };
});

builder.Services.AddAuthorization();

// Quartz schedule (every 15 minutes)
builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("SyncJob");
    q.AddJob<SyncJob>(opts => opts.WithIdentity(jobKey));

    q.AddTrigger(t => t
        .ForJob(jobKey)
        .WithIdentity("SyncJob-trigger")
        .WithSimpleSchedule(s => s.WithInterval(TimeSpan.FromMinutes(15)).RepeatForever()));
});
builder.Services.AddQuartzHostedService(opt => opt.WaitForJobsToComplete = true);

// Wiring
builder.Services.AddDevPulseInfrastructure(cfg);
builder.Services.AddDevPulseApplication();

// CORS for SPA — NOTE AllowCredentials() is required for cookies (OAuth state)
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// HttpClient (used by GitHub OAuth calls)
builder.Services.AddHttpClient();

// CORS for SPA
builder.Services.AddCors(p => p.AddPolicy("spa",
    b => b
        .WithOrigins("http://localhost:5173")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
));

var app = builder.Build();
app.UseCors("spa");

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("spa");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/healthz", () => Results.Ok(new { ok = true, at = DateTimeOffset.UtcNow }));

app.Run();