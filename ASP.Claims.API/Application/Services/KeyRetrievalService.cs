namespace ASP.Claims.API.Application.Services;

using ASP.Claims.API.Settings;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

public static class KeyRetrievalService
{
    public static async Task<string> GetJwtKeyAsync(IConfiguration config, IWebHostEnvironment env)
    {
        if (env.IsEnvironment("Test"))
            return config["TestJwt:TestKey"] ?? Environment.GetEnvironmentVariable("TestJwt__TestKey")!;

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
        if (env.IsEnvironment("Test"))
            return config["CosmosDb:Key"] ?? Environment.GetEnvironmentVariable("CosmosDb__Key")!;

        var keyVaultSettings = config.GetSection("KeyVault").Get<KeyVaultSettings>()
            ?? throw new InvalidOperationException("KeyVault section is missing in configuration.");

        if (string.IsNullOrWhiteSpace(keyVaultSettings.Url) || string.IsNullOrWhiteSpace(keyVaultSettings.CosmosDbPrimaryKeySecretName))
            throw new InvalidOperationException("KeyVault:Url or KeyVault:CosmosDbPrimaryKeySecretName is missing or empty.");

        var client = new SecretClient(new Uri(keyVaultSettings.Url), new DefaultAzureCredential());
        KeyVaultSecret secret = await client.GetSecretAsync(keyVaultSettings.CosmosDbPrimaryKeySecretName);
        return secret.Value ?? throw new InvalidOperationException("Key Vault returned a null Cosmos DB secret value.");
    }

    public static async Task<string> GetApplicationURLAsync(IConfiguration config, IWebHostEnvironment env)
    {
        if (env.IsEnvironment("Test"))
            return config["CosmosDb:Key"] ?? Environment.GetEnvironmentVariable("CosmosDb__Key")!;

        var keyVaultSettings = config.GetSection("KeyVault").Get<KeyVaultSettings>()
            ?? throw new InvalidOperationException("KeyVault section is missing in configuration.");

        if (string.IsNullOrWhiteSpace(keyVaultSettings.Url))
            throw new InvalidOperationException("KeyVault:Url is missing or empty.");

        var client = new SecretClient(new Uri(keyVaultSettings.Url), new DefaultAzureCredential());
        KeyVaultSecret secret = await client.GetSecretAsync(keyVaultSettings.Url);
        return secret.Value ?? throw new InvalidOperationException("Key Vault returned a null URL");
    }
}