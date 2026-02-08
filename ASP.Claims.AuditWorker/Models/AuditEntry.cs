namespace ASP.Claims.AuditWorker.Models;

using System.Text.Json.Serialization;

// Example of what you'd do in production
public class AuditEntry
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [JsonPropertyName("partitionKey")]
    public string PartitionKey => EventType; // For Cosmos DB partitioning
    
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
    
    // Compliance fields
    public string UserId { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    
    // Metadata
    [JsonPropertyName("metadata")]
    public Dictionary<string, object> Metadata { get; set; } = [];
}
