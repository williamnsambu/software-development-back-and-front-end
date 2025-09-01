using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using DevPulse.Infrastructure;
using Quartz;

var builder = WebApplication.CreateBuilder(args);
var cfg = builder.Configuration;

// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Data Protection (for token encryption helpers, etc.)
builder.Services.AddDataProtection();

// Auth (JWT)
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = cfg["Auth:Issuer"],
        ValidAudience = cfg["Auth:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(cfg["Auth:SigningKey"] ?? throw new InvalidOperationException("Auth:SigningKey missing")))
    };
});

builder.Services.AddAuthorization();

// Quartz (optional: keep if you already have a SyncJob)
builder.Services.AddQuartz(q =>
{
    // var jobKey = new JobKey("SyncJob");
    // q.AddJob<SyncJob>(opts => opts.WithIdentity(jobKey));
    // q.AddTrigger(t => t.ForJob(jobKey)
    //     .WithSimpleSchedule(s => s.WithInterval(TimeSpan.FromMinutes(5)).RepeatForever()));
});
builder.Services.AddQuartzHostedService(opt => opt.WaitForJobsToComplete = true);

// Bring in Infrastructure wiring (DbContext, HttpClients, Options, JwtIssuer, OAuth…)
builder.Services.AddDevPulseInfrastructure(cfg);

// CORS for SPA later
builder.Services.AddCors(p => p.AddPolicy("spa",
    b => b.WithOrigins("http://localhost:5173").AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("spa");
app.UseAuthentication();
app.UseAuthorization();

// Use controllers (you already have AuthController, ProvidersController, DashboardController, WebhooksController)
app.MapControllers();

// Simple health endpoint
app.MapGet("/healthz", () => Results.Ok(new { ok = true, at = DateTimeOffset.UtcNow }));

app.Run();