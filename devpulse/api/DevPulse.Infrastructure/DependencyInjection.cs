using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

using DevPulse.Application.Abstractions;
using DevPulse.Application.Options;
using DevPulse.Infrastructure.Persistence;
using DevPulse.Infrastructure.Providers.Github;
using DevPulse.Infrastructure.Security;
using Polly;
using Polly.Extensions.Http;

namespace DevPulse.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddDevPulseInfrastructure(
        this IServiceCollection services,
        IConfiguration cfg)
    {
        // EF Core — SQL Server
        services.AddDbContext<AppDbContext>(o =>
            o.UseSqlServer(cfg.GetConnectionString("db")));

        // Options
        services.Configure<AuthOptions>(cfg.GetSection("Auth"));
        services.Configure<GithubOptions>(cfg.GetSection("GitHub"));
        services.Configure<JiraOptions>(cfg.GetSection("Jira"));
        services.Configure<OpenWeatherOptions>(cfg.GetSection("OpenWeather"));

        // HttpClient(s) + Polly
        services.AddHttpClient("github")
            .AddPolicyHandler(HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(3, i => TimeSpan.FromMilliseconds(200 * i)));

        // Concrete infra services
        services.AddScoped<GithubWebhookService>();

        // Abstractions implemented in Infrastructure
        services.AddScoped<IOAuthService, GithubOAuthService>();
        services.AddScoped<IJwtIssuer, JwtIssuer>();

        return services;
    }
}