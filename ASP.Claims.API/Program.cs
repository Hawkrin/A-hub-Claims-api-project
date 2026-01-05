using Asp.Versioning;
using ASP.Claims.API.Extensions;
using ASP.Claims.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.ConfigureAppConfiguration();

var jwtKey = await builder.Configuration.GetJwtKeyAsync(builder.Environment);
var cosmosDbKey = await builder.Configuration.GetCosmosDbKeyAsync(builder.Environment);

builder.Services.AddJwtAndAppServices(jwtKey, cosmosDbKey, builder.Environment.IsEnvironment("Test"), builder.Configuration);

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

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.MapOpenApi();

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.MapControllers();
app.Run();

public partial class Program { }