using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace RutaViva.Api.Tests;

public sealed class HealthEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public HealthEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Health_returns_ok()
    {
        HttpResponseMessage response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Swagger_document_describes_the_fleet_routes()
    {
        HttpResponseMessage response = await _client.GetAsync("/swagger/v1/swagger.json");

        string document = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == HttpStatusCode.OK, document);
        Assert.Contains("\"/api/fleet\"", document);
        Assert.Contains("\"/api/telemetry/readings\"", document);
        Assert.Contains("\"/api/system/daily-review\"", document);
        Assert.Contains("RutaViva Fleet Maintenance API", document);
    }
}
