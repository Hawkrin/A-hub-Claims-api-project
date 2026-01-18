using ASP.Claims.API.API.DTOs;
using ASP.Claims.API.Domain.Enums;
using ASP.Claims.API.Test.Setup;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace ASP.Claims.API.Test.ControllerTests;

public class AuthControllerTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();
    private const string ValidUsername = "testuser";
    private const string ValidPassword = "TestPassword123!";
    private const Role ValidRole = Role.Admin;

    private async Task RegisterTestUser()
    {
        var registerDto = new RegisterDto
        {
            Username = ValidUsername,
            Password = ValidPassword,
            Role = ValidRole
        };
        await _client.PostAsJsonAsync("/api/Auth/register", registerDto);
    }

    [Fact]
    public async Task Register_ReturnsOk_ForNewUser()
    {
        var dto = new RegisterDto
        {
            Username = "newuser_f128ead62d7e4701a7f3754c3c188c56",
            Password = "NewUserPassword1!",
            Role = Role.User
        };
        var response = await _client.PostAsJsonAsync("/api/Auth/register", dto);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Register_ReturnsBadRequest_ForDuplicateUser()
    {
        var dto = new RegisterDto
        {
            Username = "duplicateuser",
            Password = "SomePassword!",
            Role = Role.User
        };
        // Register once
        await _client.PostAsJsonAsync("/api/Auth/register", dto);
        // Register again
        var response = await _client.PostAsJsonAsync("/api/Auth/register", dto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    //[Fact]
    //public async Task GenerateToken_ReturnsOk_ForValidCredentials()
    //{
    //    await RegisterTestUser();
    //    var login = new LoginDto
    //    {
    //        Username = ValidUsername,
    //        Password = ValidPassword
    //    };
    //    var response = await _client.PostAsJsonAsync("/api/Auth/token", login);
    //    response.StatusCode.Should().Be(HttpStatusCode.OK);
    //    var content = await response.Content.ReadAsStringAsync();
    //    content.Should().Contain("token");
    //}

    //[Fact]
    //public async Task Login_ReturnsOk_ForValidCredentials()
    //{
    //    await RegisterTestUser();
    //    var login = new LoginDto
    //    {
    //        Username = ValidUsername,
    //        Password = ValidPassword
    //    };
    //    var response = await _client.PostAsJsonAsync("/api/Auth/login", login);
    //    response.StatusCode.Should().Be(HttpStatusCode.OK);
    //    var content = await response.Content.ReadAsStringAsync();
    //    content.Should().Contain("token");
    //}

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
        await RegisterTestUser();
        var login = new LoginDto
        {
            Username = ValidUsername,
            Password = "wrongpassword"
        };
        var response = await _client.PostAsJsonAsync("/api/Auth/token", login);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_ForInvalidCredentials()
    {
        var login = new LoginDto
        {
            Username = "notadmin",
            Password = "wrongpassword"
        };
        var response = await _client.PostAsJsonAsync("/api/Auth/login", login);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
