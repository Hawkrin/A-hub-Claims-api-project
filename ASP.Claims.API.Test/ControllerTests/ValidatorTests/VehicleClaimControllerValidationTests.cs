using ASP.Claims.API.API.DTOs;
using ASP.Claims.API.API.DTOs.Claims;
using ASP.Claims.API.Domain.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace ASP.Claims.API.Test.ControllerTests.ValidatorTests;

public class VehicleClaimControllerValidationTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenRegistrationNumberIsEmpty()
    {
        var dto = new VehicleClaimDto
        {
            RegistrationNumber = "",
            PlaceOfAccident = "Test Place",
            ReportedDate = DateTime.UtcNow,
            Description = "Test",
            Status = ClaimStatus.None
        };

        var response = await _client.PostAsJsonAsync("/api/VehicleClaim", dto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Registreringsnummer saknas.");
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenRegistrationNumberFormatIsInvalid()
    {
        var dto = new VehicleClaimDto
        {
            RegistrationNumber = "INVALID",
            PlaceOfAccident = "Test Place",
            ReportedDate = DateTime.UtcNow,
            Description = "Test",
            Status = ClaimStatus.None
        };

        var response = await _client.PostAsJsonAsync("/api/VehicleClaim", dto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Fel Registreringsnummerformat");
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenPlaceOfAccidentIsEmpty()
    {
        var dto = new VehicleClaimDto
        {
            RegistrationNumber = "ABC123",
            PlaceOfAccident = "",
            ReportedDate = DateTime.UtcNow,
            Description = "Test",
            Status = ClaimStatus.None
        };

        var response = await _client.PostAsJsonAsync("/api/VehicleClaim", dto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("'Place Of Accident' must not be empty.");
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenPlaceOfAccidentIsTooLong()
    {
        var dto = new VehicleClaimDto
        {
            RegistrationNumber = "ABC123",
            PlaceOfAccident = new string('a', 201),
            ReportedDate = DateTime.UtcNow,
            Description = "Test",
            Status = ClaimStatus.None
        };

        var response = await _client.PostAsJsonAsync("/api/VehicleClaim", dto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("The length of 'Place Of Accident' must be 200 characters or fewer");
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenDescriptionIsEmpty()
    {
        var dto = new VehicleClaimDto
        {
            RegistrationNumber = "ABC123",
            PlaceOfAccident = "Test Place",
            ReportedDate = DateTime.UtcNow,
            Description = "",
            Status = ClaimStatus.None
        };

        var response = await _client.PostAsJsonAsync("/api/VehicleClaim", dto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("'Description' must not be empty.");
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenDescriptionIsTooLong()
    {
        var dto = new VehicleClaimDto
        {
            RegistrationNumber = "ABC123",
            PlaceOfAccident = "Test Place",
            ReportedDate = DateTime.UtcNow,
            Description = new string('a', 501),
            Status = ClaimStatus.None
        };

        var response = await _client.PostAsJsonAsync("/api/VehicleClaim", dto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("The length of 'Description' must be 500 characters or fewer");
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenStatusIsInvalid()
    {
        var dto = new VehicleClaimDto
        {
            RegistrationNumber = "ABC123",
            PlaceOfAccident = "Test Place",
            ReportedDate = DateTime.UtcNow,
            Description = "Test",
            Status = (ClaimStatus)999
        };

        var response = await _client.PostAsJsonAsync("/api/VehicleClaim", dto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("'Status' has a range of values which does not include '999'.");
    }
}