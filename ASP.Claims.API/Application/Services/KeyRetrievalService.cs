namespace ASP.Claims.API.Application.Services;

using ASP.Claims.API.Settings;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

public static class KeyRetrievalService
{
    // Azure Cosmos DB Emulator well-known key (same for all installations)
    private const string CosmosEmulatorWellKnownKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

    public static async Task<string> GetJwtKeyAsync(IConfiguration config, IWebHostEnvironment env)
    {
        if (env.IsEnvironment("Test"))
        {
            var testKey = config["TestJwt:TestKey"] ?? Environment.GetEnvironmentVariable("TestJwt__TestKey");
            if (!string.IsNullOrWhiteSpace(testKey))
                return testKey;

            throw new InvalidOperationException("TestJwt:TestKey is missing in test configuration.");
        }

        if (env.IsDevelopment())
        {
            var localKey = config["Jwt:Key"];
            if (!string.IsNullOrWhiteSpace(localKey))
                return localKey;
        }

        var keyVaultSettings = config.GetSection("KeyVault").Get<KeyVaultSettings>()
            ?? throw new InvalidOperationException("KeyVault section is missing in configuration.");

        if (string.IsNullOrWhiteSpace(keyVaultSettings.Url) || string.IsNullOrWhiteSpace(keyVaultSettings.JwtSecretName))
            throw new InvalidOperationException("KeyVault:Url or KeyVault:JwtSecretName is missing or empty.");

        var client = new SecretClient(new Uri(keyVaultSettings.Url), new DefaultAzureCredential());
        KeyVaultSecret secret = await client.GetSecretAsync(keyVaultSettings.JwtSecretName);
        return secret.Value ?? throw new InvalidOperationException("Key Vault returned a null JWT secret value.");
    }

    public static async Task<string> GetCosmosDbKeyAsync(IConfiguration config, IWebHostEnvironment env)
    {
        var configuredKey = config["CosmosDb:Key"] ?? Environment.GetEnvironmentVariable("CosmosDb__Key");

        // Dev/Test: allow using emulator key without requiring configuration.
        if (env.IsDevelopment() || env.IsEnvironment("Test"))
        {
            if (!string.IsNullOrWhiteSpace(configuredKey))
                return configuredKey;

            return CosmosEmulatorWellKnownKey;
        }

        var keyVaultSettings = config.GetSection("KeyVault").Get<KeyVaultSettings>()
            ?? throw new InvalidOperationException("KeyVault section is missing in configuration.");

        if (string.IsNullOrWhiteSpace(keyVaultSettings.Url) || string.IsNullOrWhiteSpace(keyVaultSettings.CosmosDbPrimaryKeySecretName))
            throw new InvalidOperationException("KeyVault:Url or KeyVault:CosmosDbPrimaryKeySecretName is missing or empty.");

        var client = new SecretClient(new Uri(keyVaultSettings.Url), new DefaultAzureCredential());
        KeyVaultSecret secret = await client.GetSecretAsync(keyVaultSettings.CosmosDbPrimaryKeySecretName);
        return secret.Value ?? throw new InvalidOperationException("Key Vault returned a null Cosmos DB secret value.");
    }
}