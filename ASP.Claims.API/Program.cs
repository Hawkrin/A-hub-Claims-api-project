using Asp.Versioning;
using ASP.Claims.API.Extensions;
using ASP.Claims.API.Middleware;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// SERVICE CONFIGURATION
// ============================================================================

// Aspire service defaults (OpenTelemetry, health checks, service discovery)
builder.AddServiceDefaults();

// Application Insights and Logging
builder.Services.AddApplicationInsightsTelemetry();
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var aiConnectionString = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
if (!string.IsNullOrWhiteSpace(aiConnectionString))
{
    builder.Logging.AddApplicationInsights(
        configureTelemetryConfiguration: (config) =>
            config.ConnectionString = aiConnectionString,
        configureApplicationInsightsLoggerOptions: (options) => { }
    );
}

// Configuration
builder.ConfigureAppConfiguration();

// JWT Key retrieval (from Key Vault in production, local config in dev)
var jwtKey = await builder.Configuration.GetJwtKeyAsync(builder.Environment);

// Redis for pub/sub messaging (Development: Aspire provides it)
if (builder.Environment.IsDevelopment())
{
    builder.AddRedisClient("ServiceBus");
}

// CORS
builder.Services.AddCorsConfiguration(builder.Environment);

// Event Publishing (Redis or No-op fallback)
builder.Services.AddEventPublishing(builder.Configuration, builder.Environment);

// JWT Authentication, Application Services, and Cosmos DB
builder.Services.AddJwtAndAppServices(jwtKey, builder.Configuration, builder.Environment);

// API Documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();

// API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
});

// HTTP Logging
builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestPath |
                           Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestMethod |
                           Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.ResponseStatusCode |
                           Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.Duration;
});

var app = builder.Build();

// ============================================================================
// MIDDLEWARE PIPELINE
// ============================================================================

// Aspire health check endpoints (/health, /alive)
app.MapDefaultEndpoints();

// Cosmos DB initialization and seeding
await app.InitializeCosmosDbAsync(app.Lifetime.ApplicationStopping);

// HTTPS redirection (production only)
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Middleware pipeline
app.UseCors();
app.UseMiddleware<RequestCorrelationMiddleware>();
app.UseHttpLogging();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ExceptionHandlingMiddleware>();

// ============================================================================
// API DOCUMENTATION (Swagger & Scalar)
// ============================================================================

app.MapOpenApi();

app.MapScalarApiReference(options =>
{
    options.Title = "Claims API";
    options.Theme = ScalarTheme.Default;
    options.DarkMode = true;

    var baseUrl = builder.Configuration["ClaimsApiBaseUrlPath"];
    if (!string.IsNullOrWhiteSpace(baseUrl))
    {
        baseUrl = baseUrl.TrimEnd('/');
        options.Servers = [new ScalarServer(baseUrl, "Claims API")];
    }
});

app.UseSwagger();
app.UseSwaggerUI();

// ============================================================================
// ENDPOINTS
// ============================================================================

// Root endpoint - redirect to Scalar API documentation
app.MapGet("/", () => Results.Redirect("/scalar/v1"))
   .ExcludeFromDescription(); // Hide from OpenAPI spec

// Map controllers
app.MapControllers();

app.Run();

public partial class Program { }
