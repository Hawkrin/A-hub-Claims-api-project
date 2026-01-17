using ASP.Claims.API.API.DTOs;
using ASP.Claims.API.Domain.Enums;
using ASP.Claims.API.Test.Setup;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace ASP.Claims.API.Test.ControllerTests.ValidatorTests;

public class AuthControllerValidationTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    #region Register Validation Tests

    [Fact]
    public async Task Register_ReturnsBadRequest_WhenUsernameIsEmpty()
    {
        var dto = new RegisterDto
        {
            Username = "",
            Password = "ValidPass123",
            Role = Role.User
        };

        var response = await _client.PostAsJsonAsync("/api/Auth/register", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Username is required");
    }

    [Fact]
    public async Task Register_ReturnsBadRequest_WhenUsernameTooShort()
    {
        var dto = new RegisterDto
        {
            Username = "ab",
            Password = "ValidPass123",
            Role = Role.User
        };

        var response = await _client.PostAsJsonAsync("/api/Auth/register", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("at least 3 characters");
    }

    [Fact]
    public async Task Register_ReturnsBadRequest_WhenUsernameContainsInvalidCharacters()
    {
        var dto = new RegisterDto
        {
            Username = "user@name!",
            Password = "ValidPass123",
            Role = Role.User
        };

        var response = await _client.PostAsJsonAsync("/api/Auth/register", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("letters, numbers, and underscores");
    }

    [Fact]
    public async Task Register_ReturnsBadRequest_WhenPasswordIsEmpty()
    {
        var dto = new RegisterDto
        {
            Username = "validuser",
            Password = "",
            Role = Role.User
        };

        var response = await _client.PostAsJsonAsync("/api/Auth/register", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Password is required");
    }

    [Fact]
    public async Task Register_ReturnsBadRequest_WhenPasswordTooShort()
    {
        var dto = new RegisterDto
        {
            Username = "validuser",
            Password = "Short1",
            Role = Role.User
        };

        var response = await _client.PostAsJsonAsync("/api/Auth/register", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("at least 8 characters");
    }

    [Fact]
    public async Task Register_ReturnsBadRequest_WhenPasswordLacksUppercase()
    {
        var dto = new RegisterDto
        {
            Username = "validuser",
            Password = "lowercase123",
            Role = Role.User
        };

        var response = await _client.PostAsJsonAsync("/api/Auth/register", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("uppercase letter");
    }

    [Fact]
    public async Task Register_ReturnsBadRequest_WhenPasswordLacksLowercase()
    {
        var dto = new RegisterDto
        {
            Username = "validuser",
            Password = "UPPERCASE123",
            Role = Role.User
        };

        var response = await _client.PostAsJsonAsync("/api/Auth/register", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("lowercase letter");
    }

    [Fact]
    public async Task Register_ReturnsBadRequest_WhenPasswordLacksDigit()
    {
        var dto = new RegisterDto
        {
            Username = "validuser",
            Password = "NoDigitsHere",
            Role = Role.User
        };

        var response = await _client.PostAsJsonAsync("/api/Auth/register", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("digit");
    }

    [Fact]
    public async Task Register_ReturnsBadRequest_WhenUsernameAlreadyExists()
    {
        var dto = new RegisterDto
        {
            Username = "duplicate_user_test",
            Password = "ValidPass123",
            Role = Role.User
        };

        // Register first time
        await _client.PostAsJsonAsync("/api/Auth/register", dto);
        
        // Register second time with same username
        var response = await _client.PostAsJsonAsync("/api/Auth/register", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Username already exists");
    }

    [Fact]
    public async Task Register_ReturnsOk_WhenAllValidationPasses()
    {
        var dto = new RegisterDto
        {
            Username = "newvaliduser",
            Password = "ValidPass123",
            Role = Role.User
        };

        var response = await _client.PostAsJsonAsync("/api/Auth/register", dto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("registered successfully");
    }

    #endregion

    #region Login Validation Tests

    [Fact]
    public async Task Login_ReturnsBadRequest_WhenUsernameIsEmpty()
    {
        var dto = new LoginDto
        {
            Username = "",
            Password = "SomePassword123"
        };

        var response = await _client.PostAsJsonAsync("/api/Auth/login", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Username is required");
    }

    [Fact]
    public async Task Login_ReturnsBadRequest_WhenPasswordIsEmpty()
    {
        var dto = new LoginDto
        {
            Username = "someuser",
            Password = ""
        };

        var response = await _client.PostAsJsonAsync("/api/Auth/login", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Password is required");
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenUsernameDoesNotExist()
    {
        var dto = new LoginDto
        {
            Username = "nonexistentuser",
            Password = "SomePassword123"
        };

        var response = await _client.PostAsJsonAsync("/api/Auth/login", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Invalid username or password");
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenPasswordIsIncorrect()
    {
        // First register a user
        var registerDto = new RegisterDto
        {
            Username = "testuser_wrong_pass",
            Password = "CorrectPass123",
            Role = Role.User
        };
        await _client.PostAsJsonAsync("/api/Auth/register", registerDto);

        // Try to login with wrong password
        var loginDto = new LoginDto
        {
            Username = "testuser_wrong_pass",
            Password = "WrongPass123"
        };

        var response = await _client.PostAsJsonAsync("/api/Auth/login", loginDto);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Invalid username or password");
    }

    [Fact]
    public async Task Login_ReturnsOk_WithToken_WhenCredentialsAreValid()
    {
        // First register a user
        var registerDto = new RegisterDto
        {
            Username = "testuser_valid_login",
            Password = "ValidPass123",
            Role = Role.User
        };
        await _client.PostAsJsonAsync("/api/Auth/register", registerDto);

        // Login with correct credentials
        var loginDto = new LoginDto
        {
            Username = "testuser_valid_login",
            Password = "ValidPass123"
        };

        var response = await _client.PostAsJsonAsync("/api/Auth/login", loginDto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("token");
    }

    #endregion
}
