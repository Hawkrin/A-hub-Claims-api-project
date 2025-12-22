using ASP.Claims.API.API.Validators;
using ASP.Claims.API.Application.CQRS.Claims.Commands;
using ASP.Claims.API.Application.Interfaces;
using ASP.Claims.API.Application.Profiles;
using ASP.Claims.API.Application.Services;
using ASP.Claims.API.Infrastructures.Repositories;
using ASP.Claims.API.Middleware.Filters;
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
            services.AddSingleton(s =>
            {
                var config = s.GetRequiredService<IConfiguration>();
                var account = config["CosmosDb:Account"];
                var databaseName = config["CosmosDb:DatabaseName"];
                var containerName = config["CosmosDb:ContainerName"];
                var cosmosClient = new CosmosClient(account, cosmosDbKey);
                return cosmosClient.GetContainer(databaseName, containerName);
            });
            services.AddSingleton<IClaimRepository, CosmosDbClaimRepository>();
            services.AddSingleton<IUserRepository, CosmosDbUserRepository>();
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
}