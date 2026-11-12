using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using RutaViva.Infrastructure.Seed;

namespace RutaViva.Api.Tests;

public sealed class FleetMaintenanceSmokeTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory = new();
    private readonly HttpClient _client;

    public FleetMaintenanceSmokeTests()
    {
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Higher_reading_is_accepted_and_updates_the_odometer()
    {
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/telemetry/readings", new
        {
            vehicleId = DemoFleet.AbcVehicleId,
            kilometers = 59_000
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        ReadingOutcome? outcome = await response.Content.ReadFromJsonAsync<ReadingOutcome>();
        Assert.Equal("Accepted", outcome!.Result);

        FleetVehicle[]? fleet = await _client.GetFromJsonAsync<FleetVehicle[]>("/api/fleet");
        Assert.Equal(59_000, fleet!.Single(vehicle => vehicle.Id == DemoFleet.AbcVehicleId).Odometer);
    }

    [Fact]
    public async Task Lower_reading_is_ignored()
    {
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/telemetry/readings", new
        {
            vehicleId = DemoFleet.QweVehicleId,
            kilometers = 20_000
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        ReadingOutcome? outcome = await response.Content.ReadFromJsonAsync<ReadingOutcome>();
        Assert.Equal("Ignored", outcome!.Result);

        FleetVehicle[]? fleet = await _client.GetFromJsonAsync<FleetVehicle[]>("/api/fleet");
        Assert.Equal(21_300, fleet!.Single(vehicle => vehicle.Id == DemoFleet.QweVehicleId).Odometer);
    }

    [Fact]
    public async Task Negative_kilometers_return_a_spanish_problem()
    {
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/telemetry/readings", new
        {
            vehicleId = DemoFleet.AbcVehicleId,
            kilometers = -1
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        ProblemBody? problem = await response.Content.ReadFromJsonAsync<ProblemBody>();
        Assert.Equal("Los kilómetros no pueden ser negativos.", problem!.Title);
    }

    [Fact]
    public async Task Starting_a_created_order_returns_conflict_and_leaves_it_created()
    {
        HttpResponseMessage response = await _client.PostAsync($"/api/workshop/orders/{DemoFleet.FrenosOrderId}/start", null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        ProblemBody? problem = await response.Content.ReadFromJsonAsync<ProblemBody>();
        Assert.Equal("La orden no admite esta acción en su estado actual.", problem!.Title);

        OrderBody? order = await _client.GetFromJsonAsync<OrderBody>($"/api/orders/{DemoFleet.FrenosOrderId}");
        Assert.Equal("Created", order!.Status);
    }

    [Fact]
    public async Task Seeded_vehicles_include_an_up_to_date_routine_besides_the_original_one()
    {
        VehicleBody? abcVehicle = await _client.GetFromJsonAsync<VehicleBody>($"/api/vehicles/{DemoFleet.AbcVehicleId}");
        VehicleBody? xyzVehicle = await _client.GetFromJsonAsync<VehicleBody>($"/api/vehicles/{DemoFleet.XyzVehicleId}");
        VehicleBody? qweVehicle = await _client.GetFromJsonAsync<VehicleBody>($"/api/vehicles/{DemoFleet.QweVehicleId}");

        Assert.Contains(abcVehicle!.Routines, routine => routine.Id == DemoFleet.AbcLlantasRoutineId && routine.Status == "UpToDate");
        Assert.Contains(xyzVehicle!.Routines, routine => routine.Id == DemoFleet.XyzAceiteRoutineId && routine.Status == "UpToDate");
        Assert.Contains(xyzVehicle.Routines, routine => routine.Id == DemoFleet.XyzLlantasRoutineId && routine.Status == "UpToDate");
        Assert.Contains(qweVehicle!.Routines, routine => routine.Id == DemoFleet.QweAceiteRoutineId && routine.Status == "UpToDate");
    }

    [Fact]
    public async Task Daily_review_on_the_due_soon_date_writes_the_event_log()
    {
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/system/daily-review", new { date = "2027-01-14" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        VehicleBody? vehicle = await _client.GetFromJsonAsync<VehicleBody>($"/api/vehicles/{DemoFleet.AbcVehicleId}");
        Assert.Equal("DueSoon", vehicle!.Routines.Single(routine => routine.Id == DemoFleet.AceiteRoutineId).Status);

        EventBody[]? events = await _client.GetFromJsonAsync<EventBody[]>("/api/system/events");
        Assert.Contains(events!, recordedEvent =>
            recordedEvent.Name == "RoutineBecameDueSoon"
            && recordedEvent.VehicleId == DemoFleet.AbcVehicleId
            && recordedEvent.RoutineId == DemoFleet.AceiteRoutineId);
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    private sealed record ReadingOutcome(string Result);

    private sealed record FleetVehicle(Guid Id, int Odometer);

    private sealed record ProblemBody(string? Title);

    private sealed record OrderBody(string Status);

    private sealed record VehicleBody(RoutineBody[] Routines);

    private sealed record RoutineBody(Guid Id, string Status);

    private sealed record EventBody(string Name, Guid? VehicleId, Guid? RoutineId);
}
