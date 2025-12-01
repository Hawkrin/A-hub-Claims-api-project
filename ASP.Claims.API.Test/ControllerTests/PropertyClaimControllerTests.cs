using ASP.Claims.API.API.DTOs;
using ASP.Claims.API.API.DTOs.Claims;
using ASP.Claims.API.Domain.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace ASP.Claims.API.Test.ControllerTests;

public class PropertyClaimControllerTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/PropertyClaim");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_ForMissingClaim()
    {
        var response = await _client.GetAsync($"/api/PropertyClaim/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_ReturnsCreated_ForValidClaim()
    {
        var dto = new PropertyClaimDto
        {
            Address = "Test Address",
            PropertyDamageType = PropertyDamageType.Fire,
            EstimatedDamageCost = 1000,
            ReportedDate = DateTime.UtcNow,
            Description = "Test",
            Status = ClaimStatus.None
        };

        var response = await _client.PostAsJsonAsync("/api/PropertyClaim", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_ForInvalidClaim()
    {
        var dto = new PropertyClaimDto(); // missing required fields
        var response = await _client.PostAsJsonAsync("/api/PropertyClaim", dto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_ReturnsNoContent_ForValidUpdate()
    {
        // First create a claim
        var dto = new PropertyClaimDto
        {
            Address = "Test Address",
            PropertyDamageType = PropertyDamageType.Fire,
            EstimatedDamageCost = 1000,
            ReportedDate = DateTime.UtcNow,
            Description = "Test",
            Status = ClaimStatus.None
        };
        var createResponse = await _client.PostAsJsonAsync("/api/PropertyClaim", dto);
        var id = await createResponse.Content.ReadFromJsonAsync<Guid>();

        // Update
        dto.Id = id;
        dto.Description = "Updated";
        var updateResponse = await _client.PutAsJsonAsync($"/api/PropertyClaim/{id}", dto);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Update_ReturnsBadRequest_ForIdMismatch()
    {
        var dto = new PropertyClaimDto { Id = Guid.NewGuid() };
        var response = await _client.PutAsJsonAsync($"/api/PropertyClaim/{Guid.NewGuid()}", dto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_IfExists()
    {
        // Create then delete
        var dto = new PropertyClaimDto
        {
            Address = "Test Address",
            PropertyDamageType = PropertyDamageType.Fire,
            EstimatedDamageCost = 1000,
            ReportedDate = DateTime.UtcNow,
            Description = "Test",
            Status = ClaimStatus.None
        };
        var createResponse = await _client.PostAsJsonAsync("/api/PropertyClaim", dto);
        var id = await createResponse.Content.ReadFromJsonAsync<Guid>();

        var deleteResponse = await _client.DeleteAsync($"/api/PropertyClaim/{id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_IfMissing()
    {
        var response = await _client.DeleteAsync($"/api/PropertyClaim/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}