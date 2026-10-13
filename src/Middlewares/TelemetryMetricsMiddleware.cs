using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace ApexGateway.Middlewares;

public sealed class TelemetryMetricsMiddleware
{
  private readonly RequestDelegate _next;
  private readonly Counter<int> _requestCounter;
  private readonly Histogram<double> _responseTimeHistogram;

  public TelemetryMetricsMiddleware(RequestDelegate next, IMeterFactory meterFactory)
  {
    _next = next;

    var meter = meterFactory.Create("ApexGateway.Metrics");
    _requestCounter = meter.CreateCounter<int>("gateway_requests_total");
    _responseTimeHistogram = meter.CreateHistogram<double>("gateway_response_time_ms");
  }

  public async Task InvokeAsync(HttpContext context)
  {
    var sw = Stopwatch.StartNew();

    try
    {
      await _next(context);
    }
    finally
    {
      sw.Stop();

      var cacheStatus = context.Response.Headers["X-Cache-Status"].ToString() ?? "NONE";
      var statusCode = context.Response.StatusCode.ToString();

      _requestCounter.Add(1,
          new KeyValuePair<string, object?>("status_code", statusCode),
          new KeyValuePair<string, object?>("cache_status", cacheStatus));

      _responseTimeHistogram.Record(sw.Elapsed.TotalMilliseconds,
          new KeyValuePair<string, object?>("route", context.Request.Path.Value ?? "unknown"));
    }
  }
}
