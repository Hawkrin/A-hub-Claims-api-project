using ASP.Claims.API.API.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace ASP.Claims.API.Test.ControllerTests;

public class AuthControllerTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GenerateToken_ReturnsOk_ForValidCredentials()
    {
        var login = new LoginDto
        {
            Username = "admin",
            Password = "password"
        };

        var response = await _client.PostAsJsonAsync("/api/Auth/token", login);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("token");
    }

    [Fact]
    public async Task GenerateToken_ReturnsUnauthorized_ForInvalidUsername()
    {
        var login = new LoginDto
        {
            Username = "notadmin",
            Password = "password"
        };

        var response = await _client.PostAsJsonAsync("/api/Auth/token", login);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GenerateToken_ReturnsUnauthorized_ForInvalidPassword()
    {
        var login = new LoginDto
        {
            Username = "admin",
            Password = "wrongpassword"
        };

        var response = await _client.PostAsJsonAsync("/api/Auth/token", login);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GenerateToken_ReturnsUnauthorized_ForInvalidCredentials()
    {
        var login = new LoginDto
        {
            Username = "notadmin",
            Password = "wrongpassword"
        };

        var response = await _client.PostAsJsonAsync("/api/Auth/token", login);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}