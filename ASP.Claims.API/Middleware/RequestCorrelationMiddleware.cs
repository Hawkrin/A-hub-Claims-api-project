namespace ASP.Claims.API.Middleware;

/// <summary>
/// Middleware that adds a correlation ID to each request for log tracking
/// </summary>
public class RequestCorrelationMiddleware(RequestDelegate next, ILogger<RequestCorrelationMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<RequestCorrelationMiddleware> _logger = logger;
    private const string CorrelationIdHeader = "X-Correlation-ID";

    public async Task Invoke(HttpContext context)
    {
        var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault() 
            ?? Guid.NewGuid().ToString();

        context.Items["CorrelationId"] = correlationId;
        context.Response.Headers.TryAdd(CorrelationIdHeader, correlationId);

        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["RequestPath"] = context.Request.Path,
            ["RequestMethod"] = context.Request.Method
        }))
        {
            await _next(context);
        }
    }
}
