using Asp.Versioning;
using ASP.Claims.API.Extensions;
using ASP.Claims.API.Middleware;
using ASP.Claims.API.Settings;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

var builder = WebApplication.CreateBuilder(args);

var keyVaultSettings = builder.Configuration.GetSection("KeyVault").Get<KeyVaultSettings>();
var client = new SecretClient(new Uri(keyVaultSettings!.Url), new DefaultAzureCredential());
KeyVaultSecret secret = await client.GetSecretAsync(keyVaultSettings.JwtSecretName);

var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>();

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddJwtAuthentication(secret.Value, jwtOptions!);

// DI regitrations
builder.Services.AddApplicationServices(secret.Value);

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