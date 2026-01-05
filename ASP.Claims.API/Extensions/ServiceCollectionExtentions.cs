using ASP.Claims.API.API.Validators;
using ASP.Claims.API.Application.CQRS.Claims.Commands;
using ASP.Claims.API.Application.Interfaces;
using ASP.Claims.API.Application.Profiles;
using ASP.Claims.API.Application.Services;
using ASP.Claims.API.Infrastructures.Repositories;
using ASP.Claims.API.Middleware.Filters;
using ASP.Claims.API.Settings;
using FluentValidation;
using Microsoft.Azure.Cosmos;
using System.Text.Json.Serialization;

namespace ASP.Claims.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, string jwtKey, string cosmosDbKey, bool isTest = false)
    {
        // Key provider and repositories
        services.AddSingleton<ITokenKeyProvider>(sp => new JwtKeyProvider(jwtKey));

        if (isTest)
        {
            services.AddSingleton<IClaimRepository, InMemoryClaimRepository>();
            services.AddSingleton<IUserRepository, InMemoryUserRepository>();
        }
        else
        {
            // Register CosmosClient as singleton
            services.AddSingleton(s =>
            {
                var config = s.GetRequiredService<IConfiguration>();
                var account = config["CosmosDb:Account"];
                return new CosmosClient(account, cosmosDbKey);
            });

            // Register Claims container
            services.AddSingleton<IClaimRepository>(s =>
            {
                var config = s.GetRequiredService<IConfiguration>();
                var cosmosClient = s.GetRequiredService<CosmosClient>();
                var dbName = config["CosmosDb:DatabaseName"];
                var containerName = config["CosmosDb:Containers:Claims"];
                var container = cosmosClient.GetContainer(dbName, containerName);
                return new CosmosDbClaimRepository(container);
            });

            // Register Users container
            services.AddSingleton<IUserRepository>(s =>
            {
                var config = s.GetRequiredService<IConfiguration>();
                var cosmosClient = s.GetRequiredService<CosmosClient>();
                var dbName = config["CosmosDb:DatabaseName"];
                var containerName = config["CosmosDb:Containers:Users"];
                var container = cosmosClient.GetContainer(dbName, containerName);
                return new CosmosDbUserRepository(container);
            });
        }

        services.AddScoped<IClaimStatusEvaluator, ClaimStatusEvaluator>();

        // AutoMapper profiles
        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<PropertyClaimMappingProfile>();
            cfg.AddProfile<TravelClaimMappingProfile>();
            cfg.AddProfile<VehicleClaimMappingProfile>();
        });

        // FluentValidation
        services.AddValidatorsFromAssemblyContaining<PropertyClaimDtoValidator>();

        // MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<CreatePropertyClaimCommand>());

        // Localization
        services.AddLocalization(options => options.ResourcesPath = "Resources");

        // Controllers and filters
        services.AddControllers(options =>
        {
            options.Filters.Add<FluentValidationActionFilter>();
        })
        .AddJsonOptions(jsonOptions =>
        {
            jsonOptions.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        return services;
    }

    public static IServiceCollection AddJwtAndAppServices(this IServiceCollection services, string jwtKey, string cosmosDbKey, bool isTest, IConfiguration configuration)
    {
        var jwtOptions = configuration.GetSection("Jwt").Get<JwtOptions>()
            ?? throw new InvalidOperationException("Jwt section is missing (Jwt:Issuer, Jwt:Audience).");

        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.AddJwtAuthentication(jwtKey, jwtOptions);
        services.AddApplicationServices(jwtKey, cosmosDbKey, isTest);

        return services;
    }
}