using ASP.Claims.API.Application.Services;
using Microsoft.Azure.Cosmos;

namespace ASP.Claims.API.Extensions;

public static class ConfigurationExtensions
{
    public static void ConfigureAppConfiguration(this WebApplicationBuilder builder)
    {
        builder.Configuration
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddJsonFile("appsettings.Test.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();
    }

    public static async Task<string> GetJwtKeyAsync(this IConfiguration config, IWebHostEnvironment env)
        => await KeyRetrievalService.GetJwtKeyAsync(config, env);

    public static async Task<string> GetCosmosDbKeyAsync(this IConfiguration config, IWebHostEnvironment env)
        => await KeyRetrievalService.GetCosmosDbKeyAsync(config, env);
}