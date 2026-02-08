using System.Text.Json.Serialization;

namespace ASP.Claims.API.API.DTOs;

public class AuditLogDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("partitionKey")]
    public string PartitionKey { get; set; } = string.Empty;
    
    [JsonPropertyName("eventType")]
    public string EventType { get; set; } = string.Empty;
    
    [JsonPropertyName("claimId")]
    public Guid ClaimId { get; set; }
    
    [JsonPropertyName("claimType")]
    public string ClaimType { get; set; } = string.Empty;
    
    [JsonPropertyName("action")]
    public string Action { get; set; } = string.Empty;
    
    [JsonPropertyName("oldValue")]
    public string OldValue { get; set; } = string.Empty;
    
    [JsonPropertyName("newValue")]
    public string NewValue { get; set; } = string.Empty;
    
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
    
    [JsonPropertyName("correlationId")]
    public string CorrelationId { get; set; } = string.Empty;
    
    [JsonPropertyName("metadata")]
    public Dictionary<string, object> Metadata { get; set; } = [];
}

public class AuditStatsDto
{
    public int TotalAuditEntries { get; set; }
    public Dictionary<string, int> EventTypeCounts { get; set; } = [];
    public DateTime LastUpdated { get; set; }
}
