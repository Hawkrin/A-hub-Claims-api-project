using ASP.Claims.API.API.DTOs;
using ASP.Claims.API.API.DTOs.Claims;
using ASP.Claims.API.Domain.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace ASP.Claims.API.Test.ControllerTests;

public class TravelClaimControllerTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/TravelClaim");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_ForMissingClaim()
    {
        var response = await _client.GetAsync($"/api/TravelClaim/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_ReturnsCreated_ForValidClaim()
    {
        var dto = new TravelClaimDto
        {
            Country = Country.Sweden,
            StartDate = DateTime.UtcNow.AddDays(-5),
            EndDate = DateTime.UtcNow,
            IncidentType = IncidentType.Delay,
            ReportedDate = DateTime.UtcNow,
            Description = "Test",
        };

        var response = await _client.PostAsJsonAsync("/api/TravelClaim", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_ForInvalidClaim()
    {
        var dto = new TravelClaimDto(); // missing required fields
        var response = await _client.PostAsJsonAsync("/api/TravelClaim", dto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_ReturnsNoContent_ForValidUpdate()
    {
        var dto = new TravelClaimDto
        {
            Id = Guid.NewGuid(),
            Country = Country.Sweden,
            StartDate = DateTime.UtcNow.AddDays(-5),
            EndDate = DateTime.UtcNow,
            IncidentType = IncidentType.Delay,
            ReportedDate = DateTime.UtcNow,
            Description = "Test",
        };

        var createResponse = await _client.PostAsJsonAsync("/api/TravelClaim", dto);
        var id = await createResponse.Content.ReadFromJsonAsync<Guid>();

        dto.Id = id;
        dto.Description = "Updated";
        var updateResponse = await _client.PutAsJsonAsync($"/api/TravelClaim/{id}", dto);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Update_ReturnsBadRequest_ForIdMismatch()
    {
        var dto = new TravelClaimDto { Id = Guid.NewGuid() };
        var response = await _client.PutAsJsonAsync($"/api/TravelClaim/{Guid.NewGuid()}", dto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_IfExists()
    {
        var dto = new TravelClaimDto
        {
            Country = Country.Sweden,
            StartDate = DateTime.UtcNow.AddDays(-5),
            EndDate = DateTime.UtcNow,
            IncidentType = IncidentType.Delay,
            ReportedDate = DateTime.UtcNow,
            Description = "Test",
            Status = ClaimStatus.None
        };

        var createResponse = await _client.PostAsJsonAsync("/api/TravelClaim", dto);
        var id = await createResponse.Content.ReadFromJsonAsync<Guid>();

        var deleteResponse = await _client.DeleteAsync($"/api/TravelClaim/{id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_IfMissing()
    {
        var response = await _client.DeleteAsync($"/api/TravelClaim/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}