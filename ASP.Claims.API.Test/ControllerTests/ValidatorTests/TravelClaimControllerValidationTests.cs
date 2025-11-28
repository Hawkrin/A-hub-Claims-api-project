using ASP.Claims.API.API.DTOs;
using ASP.Claims.API.Domain.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace ASP.Claims.API.Test.ControllerTests.ValidatorTests;

public class TravelClaimControllerValidationTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenDescriptionIsEmpty()
    {
        var dto = new TravelClaimDto
        {
            Country = Country.Sweden,
            StartDate = DateTime.UtcNow.AddDays(-5),
            EndDate = DateTime.UtcNow,
            IncidentType = IncidentType.Delay,
            ReportedDate = DateTime.UtcNow,
            Description = ""
        };

        var response = await _client.PostAsJsonAsync("/api/TravelClaim", dto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("'Description' must not be empty.");
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenDescriptionIsTooLong()
    {
        var dto = new TravelClaimDto
        {
            Country = Country.Sweden,
            StartDate = DateTime.UtcNow.AddDays(-5),
            EndDate = DateTime.UtcNow,
            IncidentType = IncidentType.Delay,
            ReportedDate = DateTime.UtcNow,
            Description = new string('a', 501)
        };

        var response = await _client.PostAsJsonAsync("/api/TravelClaim", dto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        // Default FluentValidation message for max length, unless you override it
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("The length of 'Description' must be 500 characters or fewer");
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenStatusIsInvalid()
    {
        var dto = new TravelClaimDto
        {
            Country = Country.Sweden,
            StartDate = DateTime.UtcNow.AddDays(-5),
            EndDate = DateTime.UtcNow,
            IncidentType = IncidentType.Delay,
            ReportedDate = DateTime.UtcNow,
            Description = "Test",
            Status = (ClaimStatus)999 // Invalid enum value
        };

        var response = await _client.PostAsJsonAsync("/api/TravelClaim", dto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        // Default FluentValidation message for IsInEnum
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("'Status' has a range of values which does not include '999'.");
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenCountryIsMissing()
    {
        var dto = new TravelClaimDto
        {
            StartDate = DateTime.UtcNow.AddDays(-5),
            EndDate = DateTime.UtcNow,
            IncidentType = IncidentType.Delay,
            ReportedDate = DateTime.UtcNow,
            Description = "Test"
        };

        var response = await _client.PostAsJsonAsync("/api/TravelClaim", dto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("'Country' must not be empty.");
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenStartDateIsMissing()
    {
        var dto = new TravelClaimDto
        {
            Country = Country.Sweden,
            EndDate = DateTime.UtcNow,
            IncidentType = IncidentType.Delay,
            ReportedDate = DateTime.UtcNow,
            Description = "Test",
        };

        var response = await _client.PostAsJsonAsync("/api/TravelClaim", dto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Startdatum saknas.");
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenEndDateIsBeforeStartDate()
    {
        var dto = new TravelClaimDto
        {
            Country = Country.Sweden,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(-5),
            IncidentType = IncidentType.Delay,
            ReportedDate = DateTime.UtcNow,
            Description = "Test"
        };

        var response = await _client.PostAsJsonAsync("/api/TravelClaim", dto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Slutdatum måste vara samma som eller efter startdatum.");
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenReportedDateIsTooLate()
    {
        var dto = new TravelClaimDto
        {
            Country = Country.Sweden,
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow.AddDays(-20),
            IncidentType = IncidentType.Delay,
            ReportedDate = DateTime.UtcNow, // More than 14 days after EndDate
            Description = "Test"
        };

        var response = await _client.PostAsJsonAsync("/api/TravelClaim", dto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Reseskadan måste rapporteras inom 14 dagar efter hemkomst.");
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenIncidentTypeIsInvalid()
    {
        var dto = new TravelClaimDto
        {
            Country = Country.Sweden,
            StartDate = DateTime.UtcNow.AddDays(-5),
            EndDate = DateTime.UtcNow,
            IncidentType = (IncidentType)999, // Invalid enum value
            ReportedDate = DateTime.UtcNow,
            Description = "Test"
        };

        var response = await _client.PostAsJsonAsync("/api/TravelClaim", dto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Typ av incident saknas.");
    }
}