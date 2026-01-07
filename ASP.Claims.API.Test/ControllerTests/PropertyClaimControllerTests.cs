using ASP.Claims.API.API.DTOs.Claims;
using ASP.Claims.API.Domain.Enums;
using ASP.Claims.API.Test.Setup;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace ASP.Claims.API.Test.ControllerTests;

public class PropertyClaimControllerTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
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

        var json = await response.Content.ReadAsStringAsync();
        Console.WriteLine(json);
        Console.WriteLine(typeof(PropertyClaimDto).AssemblyQualifiedName);

        var createdDto = await response.Content.ReadFromJsonAsync<PropertyClaimDto>(new System.Text.Json.JsonSerializerOptions { Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() } }
);


        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_ForInvalidClaim()
    {
        var dto = new PropertyClaimDto(); // missing required fields
        var response = await _client.PostAsJsonAsync("/api/PropertyClaim", dto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

//    [Fact]
//    public async Task Update_ReturnsNoContent_ForValidUpdate()
//    {
//        // First create a claim
//        var dto = new PropertyClaimDto
//        {
//            Address = "Test Address",
//            PropertyDamageType = PropertyDamageType.Fire,
//            EstimatedDamageCost = 1000,
//            ReportedDate = DateTime.UtcNow,
//            Description = "Test",
//            Status = ClaimStatus.None
//        };
//        var createResponse = await _client.PostAsJsonAsync("/api/PropertyClaim", dto);
//        var createdDto = await createResponse.Content.ReadFromJsonAsync<PropertyClaimDto>(new System.Text.Json.JsonSerializerOptions { Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() } }
//);
//        var id = createdDto!.Id;

//        // Update
//        dto.Id = id;
//        dto.Description = "Updated";
//        var updateResponse = await _client.PutAsJsonAsync($"/api/PropertyClaim/{id}", dto);
//        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
//    }

    [Fact]
    public async Task Update_ReturnsBadRequest_ForIdMismatch()
    {
        var dto = new PropertyClaimDto { Id = Guid.NewGuid() };
        var response = await _client.PutAsJsonAsync($"/api/PropertyClaim/{Guid.NewGuid()}", dto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    //[Fact]
    //public async Task Delete_ReturnsNoContent_IfExists()
    //{
    //    // Create then delete
    //    var dto = new PropertyClaimDto
    //    {
    //        Address = "Test Address",
    //        PropertyDamageType = PropertyDamageType.Fire,
    //        EstimatedDamageCost = 1000,
    //        ReportedDate = DateTime.UtcNow,
    //        Description = "Test",
    //        Status = ClaimStatus.None
    //    };

    //    var createResponse = await _client.PostAsJsonAsync("/api/PropertyClaim", dto);
    //    var createdDto = await createResponse.Content.ReadFromJsonAsync<PropertyClaimDto>();
    //    var id = createdDto.Id;

    //    var deleteResponse = await _client.DeleteAsync($"/api/PropertyClaim/{id}");
    //    deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    //}

    [Fact]
    public async Task Delete_ReturnsNotFound_IfMissing()
    {
        var response = await _client.DeleteAsync($"/api/PropertyClaim/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    //[Fact]
    //public async Task GetAll_ReturnsUnauthorized_IfNotAuthenticated()
    //{
    //    var factory = new WebApplicationFactory<Program>();
    //    var client = factory.CreateClient();

    //    var response = await client.GetAsync("/api/PropertyClaim");
    //    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    //}
}