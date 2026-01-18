using ASP.Claims.API.API.Validators;
using ASP.Claims.API.Application;
using ASP.Claims.API.Application.CQRS.Claims.Commands;
using ASP.Claims.API.Application.Interfaces;
using ASP.Claims.API.Application.Profiles;
using ASP.Claims.API.Application.Services;
using ASP.Claims.API.Infrastructures.Repositories;
using ASP.Claims.API.Middleware.Filters;
using ASP.Claims.API.Settings;
using FluentValidation;
using Microsoft.Azure.Cosmos;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ASP.Claims.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, string jwtKey, string cosmosDbKey, bool isTest = false, IWebHostEnvironment? env = null)
    {
        services.AddSingleton<ITokenKeyProvider>(sp => new JwtKeyProvider(jwtKey));

        // Use InMemory repositories for Test AND Development
        bool useInMemory = isTest || (env?.IsDevelopment() ?? false);

        if (useInMemory)
        {
            services.AddSingleton<IClaimRepository, InMemoryClaimRepository>();
            services.AddSingleton<IUserRepository, InMemoryUserRepository>();
        }
        else
        {
            services.AddSingleton(sp =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                var account = config["CosmosDb:Account"];

                //var jsonOptions = new JsonSerializerOptions
                //{
                //    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                //};

                //var cosmosOptions = new CosmosClientOptions
                //{
                //    Serializer = new CosmosSystemTextJsonSerializer(jsonOptions)
                //};

                //return new CosmosClient(account, cosmosDbKey, cosmosOptions);

                return new CosmosClient(account, cosmosDbKey);
            });

            services.AddCosmosRepository<IClaimRepository, CosmosDbClaimRepository>("CosmosDb:DatabaseName", "CosmosDb:Containers:Claims");
            services.AddCosmosRepository<IUserRepository, CosmosDbUserRepository>("CosmosDb:DatabaseName","CosmosDb:Containers:Users");
        }

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

    public static IServiceCollection AddJwtAndAppServices(this IServiceCollection services, string jwtKey, string cosmosDbKey, bool isTest, IConfiguration configuration, IWebHostEnvironment? env = null)
    {
        var jwtOptions = configuration.GetSection("Jwt").Get<JwtOptions>()
            ?? throw new InvalidOperationException("Jwt section is missing (Jwt:Issuer, Jwt:Audience).");

        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.AddJwtAuthentication(jwtKey, jwtOptions);
        services.AddApplicationServices(jwtKey, cosmosDbKey, isTest, env);

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