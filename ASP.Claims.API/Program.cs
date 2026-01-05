using Asp.Versioning;
using ASP.Claims.API.Extensions;
using ASP.Claims.API.Middleware;
using ASP.Claims.API.Settings;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Base + environment-specific + test (for local/CI tests)
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddJsonFile("appsettings.Test.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

var jwtKey = await KeyRetrievalService.GetJwtKeyAsync(builder.Configuration, builder.Environment);
var cosmosDbKey = await KeyRetrievalService.GetCosmosDbKeyAsync(builder.Configuration, builder.Environment);

var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()
    ?? throw new InvalidOperationException("Jwt section is missing (Jwt:Issuer, Jwt:Audience).");

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddJwtAuthentication(jwtKey, jwtOptions);

// DI registrations
builder.Services.AddApplicationServices(jwtKey, cosmosDbKey, builder.Environment.IsEnvironment("Test"));

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

app.UseSwagger();
app.UseSwaggerUI();
app.MapOpenApi();

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

// Keep for tests
public partial class Program { }