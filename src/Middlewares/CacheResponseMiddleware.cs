using StackExchange.Redis;

namespace ApexGateway.Middlewares;

public sealed class CacheResponseMiddleware
{
  private readonly RequestDelegate _next;

  public CacheResponseMiddleware(RequestDelegate next)
  {
    _next = next;
  }

  public async Task InvokeAsync(HttpContext context, IConnectionMultiplexer redis)
  {
    if (!HttpMethods.IsGet(context.Request.Method))
    {
      await _next(context);
      return;
    }

    var db = redis.GetDatabase();
    var cacheKey = $"cache:{context.Request.Path}{context.Request.QueryString}";

    var cachedResponse = await db.StringGetAsync(cacheKey);

    if (cachedResponse.HasValue)
    {
      context.Response.ContentType = "application/json";
      context.Response.Headers.Append("X-Cache-Status", "HIT");
      await context.Response.WriteAsync(cachedResponse.ToString());
      return;
    }

    context.Response.Headers.Append("X-Cache-Status", "MISS");
    await _next(context);
  }
}
