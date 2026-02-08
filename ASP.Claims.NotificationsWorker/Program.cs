using ASP.Claims.NotificationsWorker;

var builder = Host.CreateApplicationBuilder(args);

// Add Aspire service defaults (telemetry, health checks)
builder.AddServiceDefaults();

// Add Redis connection for pub/sub
builder.AddRedisClient("ServiceBus");

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
