using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using RutaViva.Application.Behaviors;
using RutaViva.Application.Commands.Shared;
using RutaViva.Application.Events;

namespace RutaViva.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            configuration.AddOpenBehavior(typeof(DomainExceptionBehavior<,>));
            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);
        services.AddScoped<IDomainEventFlusher, DomainEventFlusher>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<IOrderWriteAccess, OrderWriteAccess>();
        return services;
    }
}
