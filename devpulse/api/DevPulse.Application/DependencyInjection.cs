using Microsoft.Extensions.DependencyInjection;

namespace DevPulse.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddDevPulseApplication(this IServiceCollection services)
    {
        // No concrete registrations here (interfaces + DTOs live in Application). It probably will tomorrow:
        // •	Adding MediatR (services.AddMediatR(...))
        // •	Adding FluentValidation (services.AddValidatorsFromAssembly(...))
        // •	Adding CQRS pipeline behaviors
        // •	Domain events → handlers wired in Application
        return services;
    }
}