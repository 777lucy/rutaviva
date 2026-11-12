using System.Reflection;
using Microsoft.OpenApi.Models;

namespace RutaViva.Api.OpenApi;

public static class SwaggerSetup
{
    public static IServiceCollection AddFleetSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "RutaViva Fleet Maintenance API",
                Version = "v1",
                Description =
                    "Preventive maintenance for one fleet. A routine is due by kilometers, by calendar months, " +
                    "or by whichever comes first. Readings below the odometer return 200 with result Ignored. " +
                    "Problem titles are in Spanish."
            });

            options.DocumentFilter<TagDescriptionsFilter>();

            Assembly assembly = typeof(SwaggerSetup).Assembly;
            string xmlPath = Path.Combine(Path.GetDirectoryName(assembly.Location) ?? AppContext.BaseDirectory, $"{assembly.GetName().Name}.xml");
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
            }
        });

        return services;
    }

    public static WebApplication UseFleetSwagger(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            return app;
        }

        app.UseSwagger();
        app.UseSwaggerUI(ui =>
        {
            ui.SwaggerEndpoint("/swagger/v1/swagger.json", "RutaViva v1");
            ui.DocumentTitle = "RutaViva Fleet Maintenance API";
        });

        return app;
    }
}
