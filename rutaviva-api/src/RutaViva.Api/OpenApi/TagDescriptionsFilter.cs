using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace RutaViva.Api.OpenApi;

public sealed class TagDescriptionsFilter : IDocumentFilter
{
    private static readonly OpenApiTag[] Tags =
    [
        new() { Name = "Client", Description = "Fleet, vehicle, and order reads, plus approve, reject, and cancel." },
        new() { Name = "Telemetry", Description = "Odometer readings. A lower or equal reading is ignored." },
        new() { Name = "Workshop", Description = "Quote, start, and complete a service order." },
        new() { Name = "System", Description = "Health, the daily review, and the published event log." }
    ];

    public void Apply(OpenApiDocument document, DocumentFilterContext context)
    {
        // Swashbuckle agrega tags después. La lista tiene que poder crecer.
        List<OpenApiTag> tags = document.Tags?.ToList() ?? [];
        foreach (OpenApiTag described in Tags)
        {
            OpenApiTag? existing = tags.FirstOrDefault(tag => tag.Name == described.Name);
            if (existing is null)
            {
                tags.Add(described);
            }
            else
            {
                existing.Description = described.Description;
            }
        }

        document.Tags = tags;
    }
}
