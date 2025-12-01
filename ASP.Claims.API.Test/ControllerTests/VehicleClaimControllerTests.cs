using ASP.Claims.API.API.DTOs.Claims;
using ASP.Claims.API.Domain.Enums;
using ASP.Claims.API.Test.Setup;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace ASP.Claims.API.Test.ControllerTests;

public class VehicleClaimControllerTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/VehicleClaim");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_ForMissingClaim()
    {
        var response = await _client.GetAsync($"/api/VehicleClaim/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_ReturnsCreated_ForValidClaim()
    {
        var dto = new VehicleClaimDto
        {
            RegistrationNumber = "ABC123",
            PlaceOfAccident = "Test Location",
            ReportedDate = DateTime.UtcNow,
            Description = "Test",
            Status = ClaimStatus.None
        };

        var response = await _client.PostAsJsonAsync("/api/VehicleClaim", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_ForInvalidClaim()
    {
        var dto = new VehicleClaimDto(); // missing required fields
        var response = await _client.PostAsJsonAsync("/api/VehicleClaim", dto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_ReturnsNoContent_ForValidUpdate()
    {
        var dto = new VehicleClaimDto
        {
            RegistrationNumber = "ABC123",
            PlaceOfAccident = "Test Location",
            ReportedDate = DateTime.UtcNow,
            Description = "Test",
            Status = ClaimStatus.None
        };

        var createResponse = await _client.PostAsJsonAsync("/api/VehicleClaim", dto);
        var id = await createResponse.Content.ReadFromJsonAsync<Guid>();

        dto.Id = id;
        dto.Description = "Updated";
        var updateResponse = await _client.PutAsJsonAsync($"/api/VehicleClaim/{id}", dto);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Update_ReturnsBadRequest_ForIdMismatch()
    {
        var dto = new VehicleClaimDto { Id = Guid.NewGuid() };
        var response = await _client.PutAsJsonAsync($"/api/VehicleClaim/{Guid.NewGuid()}", dto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_IfExists()
    {
        var dto = new VehicleClaimDto
        {
            RegistrationNumber = "ABC123",
            PlaceOfAccident = "Test Location",
            ReportedDate = DateTime.UtcNow,
            Description = "Test",
            Status = ClaimStatus.None
        };

        var createResponse = await _client.PostAsJsonAsync("/api/VehicleClaim", dto);
        var id = await createResponse.Content.ReadFromJsonAsync<Guid>();

        var deleteResponse = await _client.DeleteAsync($"/api/VehicleClaim/{id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_IfMissing()
    {
        var response = await _client.DeleteAsync($"/api/VehicleClaim/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAll_ReturnsUnauthorized_IfNotAuthenticated()
    {
        // Create a client without the test auth handler
        var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/VehicleClaim");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

}
