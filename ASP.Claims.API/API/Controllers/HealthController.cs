using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;

namespace ASP.Claims.API.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly TelemetryClient _telemetryClient;

    public HealthController(TelemetryClient telemetryClient)
    {
        _telemetryClient = telemetryClient;
    }

    [HttpGet]
    public IActionResult Get()
    {
        // Track a custom event to verify Application Insights is working
        _telemetryClient.TrackEvent("HealthCheck", new Dictionary<string, string>
        {
            { "Status", "Healthy" },
            { "Timestamp", DateTime.UtcNow.ToString("O") }
        });

        return Ok(new
        {
            status = "Healthy",
            timestamp = DateTime.UtcNow,
            applicationInsightsEnabled = _telemetryClient.IsEnabled()
        });
    }
}
