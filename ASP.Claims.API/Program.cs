using Asp.Versioning;
using ASP.Claims.API.Extensions;
using ASP.Claims.API.Middleware;
using ASP.Claims.API.Settings;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

var builder = WebApplication.CreateBuilder(args);

// Base + environment-specific + test (for local/CI tests)
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddJsonFile("appsettings.Test.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

string jwtKey;
if (builder.Environment.IsEnvironment("Test"))
{
    // Use a test key for CI/test/dev
    jwtKey = builder.Configuration["TestJwt:TestKey"]
             ?? throw new InvalidOperationException("TestJwt:TestKey is missing in configuration.");
}
else
{
    var keyVaultSettings = builder.Configuration.GetSection("KeyVault").Get<KeyVaultSettings>()
        ?? throw new InvalidOperationException("KeyVault section is missing in configuration.");

    if (string.IsNullOrWhiteSpace(keyVaultSettings.Url) ||
        string.IsNullOrWhiteSpace(keyVaultSettings.JwtSecretName))
    {
        throw new InvalidOperationException("KeyVault:Url or KeyVault:JwtSecretName is missing or empty.");
    }

    var client = new SecretClient(new Uri(keyVaultSettings.Url), new DefaultAzureCredential());
    KeyVaultSecret secret = await client.GetSecretAsync(keyVaultSettings.JwtSecretName);

    jwtKey = secret.Value
             ?? throw new InvalidOperationException("Key Vault returned a null JWT secret value.");
}

// Ensure Jwt section (Issuer/Audience) exists
var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()
    ?? throw new InvalidOperationException("Jwt section is missing (Jwt:Issuer, Jwt:Audience).");

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddJwtAuthentication(jwtKey, jwtOptions);

// DI registrations
builder.Services.AddApplicationServices(jwtKey);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0); // Default: v1.0
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true; // Adds API version headers to responses
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

// Keep for tests
public partial class Program { }