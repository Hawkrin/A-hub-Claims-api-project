using ASP.Claims.API.API.Validators;
using ASP.Claims.API.Application;
using ASP.Claims.API.Application.CQRS.Claims.Commands;
using ASP.Claims.API.Application.Interfaces;
using ASP.Claims.API.Application.Profiles;
using ASP.Claims.API.Application.Services;
using ASP.Claims.API.Infrastructure.Messaging;
using ASP.Claims.API.Infrastructures.Repositories;
using ASP.Claims.API.Middleware.Filters;
using ASP.Claims.API.Settings;
using FluentValidation;
using Microsoft.Azure.Cosmos;
using System.Text.Json.Serialization;

namespace ASP.Claims.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, string jwtKey)
    {
        services.AddSingleton<ITokenKeyProvider>(_ => new JwtKeyProvider(jwtKey));
        services.AddScoped<IClaimStatusEvaluator, ClaimStatusEvaluator>();

        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<PropertyClaimMappingProfile>();
            cfg.AddProfile<TravelClaimMappingProfile>();
            cfg.AddProfile<VehicleClaimMappingProfile>();
        });

        services.AddValidatorsFromAssemblyContaining<PropertyClaimDtoValidator>();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<CreatePropertyClaimCommand>());
        services.AddLocalization(options => options.ResourcesPath = "Resources");

        services.AddControllers(options =>
        {
            options.Filters.Add<FluentValidationActionFilter>();
            options.Filters.Add<LoggingActionFilter>();
        })
        .AddJsonOptions(jsonOptions =>
        {
            jsonOptions.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        return services;
    }

    public static IServiceCollection AddJwtAndAppServices(this IServiceCollection services, string jwtKey, IConfiguration configuration, IWebHostEnvironment? env = null)
    {
        var jwtOptions = configuration.GetSection("Jwt").Get<JwtOptions>()
            ?? throw new InvalidOperationException("Jwt section is missing (Jwt:Issuer, Jwt:Audience).");

        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.AddJwtAuthentication(jwtKey, jwtOptions);
        services.AddApplicationServices(jwtKey);

        var useCosmosInDevOrTest = (env?.IsDevelopment() ?? false) || (env?.IsEnvironment("Test") ?? false);
        
        // Check if CosmosClient is registered (by Aspire or manually)
        var cosmosClientRegistered = services.Any(sd => sd.ServiceType == typeof(CosmosClient));
        
        var cosmosConfigured = cosmosClientRegistered || 
            (!string.IsNullOrWhiteSpace(configuration["CosmosDb:Account"])
            && !string.IsNullOrWhiteSpace(configuration["CosmosDb:DatabaseName"])
            && !string.IsNullOrWhiteSpace(configuration["CosmosDb:Containers:Claims"])
            && !string.IsNullOrWhiteSpace(configuration["CosmosDb:Containers:Users"]));

        // COSMOS DB MODE (Default for local development with Aspire)
        // The emulator will auto-start when running via Aspire AppHost
        
        // TO USE IN-MEMORY MODE INSTEAD: Uncomment the block below and comment out Cosmos setup
        /*
        // If dev/test and Cosmos isn't configured, fall back to in-memory.
        // Production always uses Cosmos.
        var useInMemory = useCosmosInDevOrTest && !cosmosConfigured;

        if (useInMemory)
        {
            services.AddSingleton<IClaimRepository, InMemoryClaimRepository>();
            services.AddSingleton<IUserRepository, InMemoryUserRepository>();
            return services;
        }
        */

        // If CosmosClient not already registered (e.g., not using Aspire), register it manually
        if (!cosmosClientRegistered)
        {
            var cosmosDbKey = KeyRetrievalService.GetCosmosDbKeyAsync(configuration, env!).GetAwaiter().GetResult();
            services.AddSingleton(_ => new CosmosClient(configuration["CosmosDb:Account"], cosmosDbKey));
        }

        services.AddCosmosRepository<IClaimRepository, CosmosDbClaimRepository>("CosmosDb:DatabaseName", "CosmosDb:Containers:Claims");
        services.AddCosmosRepository<IUserRepository, CosmosDbUserRepository>("CosmosDb:DatabaseName", "CosmosDb:Containers:Users");

        return services;
    }

    public static IServiceCollection AddCosmosRepository<TInterface, TImplementation>(this IServiceCollection services, string dbNameConfigKey, string containerNameConfigKey)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        services.AddSingleton<TImplementation>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var cosmosClient = sp.GetRequiredService<CosmosClient>();
            var dbName = config[dbNameConfigKey];
            var containerName = config[containerNameConfigKey];
            var container = cosmosClient.GetContainer(dbName, containerName);
            return (TImplementation)Activator.CreateInstance(typeof(TImplementation), container)!;
        });

        services.AddSingleton<TInterface>(sp => sp.GetRequiredService<TImplementation>());
        return services;
    }
}
