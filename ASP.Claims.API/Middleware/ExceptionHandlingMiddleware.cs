namespace ASP.Claims.API.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger = logger;

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred. Path: {Path}, Method: {Method}", 
                context.Request.Path, context.Request.Method);
            
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            var error = new { Message = "An unexpected error occurred." };
            await context.Response.WriteAsJsonAsync(error);
        }
    }
}
