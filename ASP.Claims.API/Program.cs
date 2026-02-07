using Asp.Versioning;
using ASP.Claims.API.Application.Interfaces;
using ASP.Claims.API.Application.Services;
using ASP.Claims.API.Extensions;
using ASP.Claims.API.Middleware;
using Microsoft.Azure.Cosmos;
using Scalar.AspNetCore;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults (OpenTelemetry, health checks, service discovery)
builder.AddServiceDefaults();

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

builder.ConfigureAppConfiguration();

var jwtKey = await builder.Configuration.GetJwtKeyAsync(builder.Environment);

// Add CosmosDB via Aspire (automatically configured via AppHost)
// Only add CosmosDB client if NOT in Development or Test (using in-memory instead)
if (!builder.Environment.IsDevelopment() && !builder.Environment.IsEnvironment("Test"))
{
    builder.AddAzureCosmosClient("ClaimsDb");
}

builder.Services.AddJwtAndAppServices(jwtKey, builder.Configuration, builder.Environment);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
});

builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestPath |
                           Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestMethod |
                           Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.ResponseStatusCode |
                           Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.Duration;
});

var app = builder.Build();

// Map Aspire default endpoints (/health, /alive)
app.MapDefaultEndpoints();

// Cosmos auto-provisioning and seeding
// Initialize database and containers if using Cosmos DB
var cosmosClient = app.Services.GetService<CosmosClient>();
if (cosmosClient != null)
{
    var dbName = app.Configuration["CosmosDb:DatabaseName"] ?? "ClaimsDb";
    var claimsContainer = app.Configuration["CosmosDb:Containers:Claims"] ?? "Claims";
    var usersContainer = app.Configuration["CosmosDb:Containers:Users"] ?? "Users";

    try
    {
        await CosmosDbInitializer.InitializeAsync(cosmosClient, dbName, claimsContainer, usersContainer, app.Lifetime.ApplicationStopping);
        app.Logger.LogInformation("CosmosDB initialized successfully - Database: {DatabaseName}, Containers: {Claims}, {Users}", 
            dbName, claimsContainer, usersContainer);

        // Seed data in Development only
        if (app.Environment.IsDevelopment())
        {
            using var scope = app.Services.CreateScope();
            var seeder = new CosmosDbSeeder(
                scope.ServiceProvider.GetRequiredService<IClaimRepository>(),
                scope.ServiceProvider.GetRequiredService<IUserRepository>(),
                scope.ServiceProvider.GetRequiredService<ILogger<CosmosDbSeeder>>());
            
            await seeder.SeedAsync(app.Lifetime.ApplicationStopping);
            app.Logger.LogInformation("CosmosDB seeding completed");
        }
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Failed to initialize or seed CosmosDB - will retry on first use");
    }
}
else
{
    app.Logger.LogInformation("Using in-memory repositories (CosmosClient not registered)");
}

// Configure middleware pipeline
// Only use HTTPS redirection in Production (Aspire uses HTTP locally)
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseMiddleware<RequestCorrelationMiddleware>();
app.UseHttpLogging();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure Swagger and Scalar (must be after middleware, before endpoint mapping)
app.MapOpenApi();

// Map Scalar at /scalar/v1 with proper configuration
app.MapScalarApiReference(options =>
{
    options.Title = "Claims API";
    options.Theme = ScalarTheme.Default;
    options.DarkMode = false;
    
    // Configure server URL from environment (injected by Aspire)
    var baseUrl = builder.Configuration["ClaimsApiBaseUrlPath"];
    if (!string.IsNullOrWhiteSpace(baseUrl))
    {
        baseUrl = baseUrl.TrimEnd('/');
        options.Servers = [new ScalarServer(baseUrl, "Claims API")];
    }
});

app.UseSwagger();
app.UseSwaggerUI();

// Redirect root to Scalar in Development/Staging
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/")
    {
        if (context.RequestServices.GetRequiredService<IHostEnvironment>().IsDevelopment() ||
            context.RequestServices.GetRequiredService<IHostEnvironment>().IsStaging())
        {
            context.Response.Redirect("/scalar/v1");
            return;
        }
        
        // In production, return API info instead
        context.Response.StatusCode = 200;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new
        {
            service = "Claims API",
            status = "Running",
            version = "v1",
            documentation = "Scalar docs not available in production"
        });
        return;
    }
    await next();
});

app.MapControllers();
app.Run();

public partial class Program { }