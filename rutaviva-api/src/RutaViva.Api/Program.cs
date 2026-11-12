using Microsoft.AspNetCore.Mvc;
using RutaViva.Api.Errors;
using RutaViva.Api.Jobs;
using RutaViva.Api.OpenApi;
using RutaViva.Application;
using RutaViva.Infrastructure;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

const string WebCorsPolicy = "web";

builder.Services.AddApplication();
builder.Services.AddInfrastructure();
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
        options.InvalidModelStateResponseFactory = context =>
            new BadRequestObjectResult(new ValidationProblemDetails(context.ModelState)
            {
                Title = "La solicitud no es válida.",
                Status = StatusCodes.Status400BadRequest
            }));
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddHostedService<FleetSeedHostedService>();
builder.Services.AddHostedService<DailyMaintenanceReviewJob>();
builder.Services.AddFleetSwagger();
builder.Services.AddCors(options =>
{
    options.AddPolicy(WebCorsPolicy, policy =>
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

WebApplication app = builder.Build();

app.UseFleetSwagger();

app.UseExceptionHandler();
app.UseCors(WebCorsPolicy);
app.MapControllers();

app.Run();

public partial class Program;
