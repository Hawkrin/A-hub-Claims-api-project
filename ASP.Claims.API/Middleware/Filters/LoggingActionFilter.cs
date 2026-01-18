namespace ASP.Claims.API.Middleware.Filters;

using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;
using System.Security.Claims;

public class LoggingActionFilter(ILogger<LoggingActionFilter> logger) : IActionFilter, IAsyncActionFilter
{
    private readonly ILogger<LoggingActionFilter> _logger = logger;

    public void OnActionExecuting(ActionExecutingContext context)
    {
        var controllerName = context.Controller.GetType().Name;
        var actionName = context.ActionDescriptor.DisplayName;
        var username = context.HttpContext.User.Identity?.Name ?? "Anonymous";
        var method = context.HttpContext.Request.Method;
        var path = context.HttpContext.Request.Path;

        // Log all action arguments (be careful with sensitive data like passwords)
        var arguments = context.ActionArguments
            .Where(arg => !IsSensitiveParameter(arg.Key))
            .ToDictionary(arg => arg.Key, arg => arg.Value);

        _logger.LogInformation(
            "Action executing: {Controller}.{Action} | User: {Username} | {Method} {Path} | Arguments: {@Arguments}",
            controllerName, actionName, username, method, path, arguments);

        // Store start time for duration calculation
        context.HttpContext.Items["ActionStartTime"] = Stopwatch.GetTimestamp();
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        var controllerName = context.Controller.GetType().Name;
        var actionName = context.ActionDescriptor.DisplayName;
        var statusCode = context.HttpContext.Response.StatusCode;
        
        // Calculate duration
        var startTime = context.HttpContext.Items["ActionStartTime"] as long?;
        var duration = startTime.HasValue 
            ? Stopwatch.GetElapsedTime(startTime.Value)
            : TimeSpan.Zero;

        if (context.Exception != null)
        {
            _logger.LogError(
                context.Exception,
                "Action failed: {Controller}.{Action} | Status: {StatusCode} | Duration: {Duration}ms",
                controllerName, actionName, statusCode, duration.TotalMilliseconds);
        }
        else if (statusCode >= 400)
        {
            _logger.LogWarning(
                "Action completed with error: {Controller}.{Action} | Status: {StatusCode} | Duration: {Duration}ms",
                controllerName, actionName, statusCode, duration.TotalMilliseconds);
        }
        else
        {
            _logger.LogInformation(
                "Action completed: {Controller}.{Action} | Status: {StatusCode} | Duration: {Duration}ms",
                controllerName, actionName, statusCode, duration.TotalMilliseconds);
        }
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        OnActionExecuting(context);
        var executedContext = await next();
        OnActionExecuted(executedContext);
    }

    private static bool IsSensitiveParameter(string parameterName)
    {
        var sensitiveParams = new[] { "password", "token", "secret", "key", "credentials" };
        return sensitiveParams.Any(sp => parameterName.Contains(sp, StringComparison.OrdinalIgnoreCase));
    }
}
