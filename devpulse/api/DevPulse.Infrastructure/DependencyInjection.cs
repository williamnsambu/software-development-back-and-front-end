using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using DevPulse.Application.Abstractions;
using DevPulse.Application.Options;
using DevPulse.Infrastructure.Persistence;
using DevPulse.Infrastructure.Providers.Github;
using DevPulse.Infrastructure.Security;
using DevPulse.Infrastructure.Services;
using Polly;
using Polly.Extensions.Http;
using DevPulse.Infrastructure.Services.Weather;

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

        // Weather (real OpenWeather client)
        services.AddHttpClient<IWeatherClient, OpenWeatherClient>();

        // Concrete infra services
        services.AddScoped<GithubWebhookService>();

        // Abstractions implemented in Infrastructure
        services.AddScoped<IOAuthService, GithubOAuthService>();
        services.AddScoped<IJwtIssuer, JwtIssuer>();

        // Application services (these fix your error)
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IProviderService, ProviderService>();
        services.AddScoped<IOAuthService, GithubOAuthService>();
        services.AddScoped<IJwtIssuer, JwtIssuer>();
        services.AddScoped<GithubWebhookService>();

        return services;
    }
}