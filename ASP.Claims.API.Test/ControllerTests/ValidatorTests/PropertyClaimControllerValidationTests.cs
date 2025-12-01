using ASP.Claims.API.API.DTOs.Claims;
using ASP.Claims.API.Domain.Enums;
using ASP.Claims.API.Test.Setup;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace ASP.Claims.API.Test.ControllerTests.ValidatorTests;

public class PropertyClaimControllerValidationTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenDescriptionIsEmpty()
    {
        var dto = new PropertyClaimDto
        {
            Address = "Test Address",
            PropertyDamageType = PropertyDamageType.Fire,
            EstimatedDamageCost = 1000,
            ReportedDate = DateTime.UtcNow,
            Description = "",
            Status = ClaimStatus.None
        };

        var response = await _client.PostAsJsonAsync("/api/PropertyClaim", dto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("'Description' must not be empty.");
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenDescriptionIsTooLong()
    {
        var dto = new PropertyClaimDto
        {
            Address = "Test Address",
            PropertyDamageType = PropertyDamageType.Fire,
            EstimatedDamageCost = 1000,
            ReportedDate = DateTime.UtcNow,
            Description = new string('a', 501),
            Status = ClaimStatus.None
        };

        var response = await _client.PostAsJsonAsync("/api/PropertyClaim", dto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("The length of 'Description' must be 500 characters or fewer");
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenStatusIsInvalid()
    {
        var dto = new PropertyClaimDto
        {
            Address = "Test Address",
            PropertyDamageType = PropertyDamageType.Fire,
            EstimatedDamageCost = 1000,
            ReportedDate = DateTime.UtcNow,
            Description = "Test",
            Status = (ClaimStatus)999
        };

        var response = await _client.PostAsJsonAsync("/api/PropertyClaim", dto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("'Status' has a range of values which does not include '999'.");
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenAddressIsEmpty()
    {
        var dto = new PropertyClaimDto
        {
            Address = "",
            PropertyDamageType = PropertyDamageType.Fire,
            EstimatedDamageCost = 1000,
            ReportedDate = DateTime.UtcNow,
            Description = "Test",
            Status = ClaimStatus.None
        };

        var response = await _client.PostAsJsonAsync("/api/PropertyClaim", dto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Adress saknas.");
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenPropertyDamageTypeIsInvalid()
    {
        var dto = new PropertyClaimDto
        {
            Address = "Test Address",
            PropertyDamageType = (PropertyDamageType)999,
            EstimatedDamageCost = 1000,
            ReportedDate = DateTime.UtcNow,
            Description = "Test",
            Status = ClaimStatus.None
        };

        var response = await _client.PostAsJsonAsync("/api/PropertyClaim", dto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Typ av egendomsskada saknas.");
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenEstimatedDamageCostIsNegative()
    {
        var dto = new PropertyClaimDto
        {
            Address = "Test Address",
            PropertyDamageType = PropertyDamageType.Fire,
            EstimatedDamageCost = -1,
            ReportedDate = DateTime.UtcNow,
            Description = "Test",
            Status = ClaimStatus.None
        };

        var response = await _client.PostAsJsonAsync("/api/PropertyClaim", dto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain(" Uppskattad konstant måste vara över 0kr.");
    }
}