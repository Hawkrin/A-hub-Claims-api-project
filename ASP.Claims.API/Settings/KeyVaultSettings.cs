namespace ASP.Claims.API.Settings;

public class KeyVaultSettings
{
    public required string Url { get; set; }
    public required string JwtSecretName { get; set; }
    public required string CosmosDbPrimaryKeySecretName { get; set; }
}
