using ApexGateway.Config;
using ApexGateway.Middlewares;
using ApexGateway.Transforms;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
  options.ConfigureEndpointDefaults(listenOptions =>
  {
    listenOptions.Protocols = HttpProtocols.Http1AndHttp2AndHttp3;
  });
});

builder.Services.AddMetrics();
builder.Services.AddGatewayRateLimiter();
builder.Services.AddSingleton<ApiKeyInjectionTransform>();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(builderContext =>
    {
      var transform = builderContext.Services.GetRequiredService<ApiKeyInjectionTransform>();
      builderContext.AddRequestTransform(transform.ApplyAsync);
    });

var app = builder.Build();

app.UseRateLimiter();
app.UseMiddleware<TelemetryMetricsMiddleware>();
app.UseMiddleware<CacheResponseMiddleware>();
app.MapReverseProxy();

app.Run();
