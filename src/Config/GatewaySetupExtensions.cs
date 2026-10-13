using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace ApexGateway.Config;

public static class GatewaySetupExtensions
{
  public static IServiceCollection AddGatewayRateLimiter(this IServiceCollection services)
  {
    services.AddRateLimiter(options =>
    {
      options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

      options.AddFixedWindowLimiter("FixedWindowPolicy", opt =>
      {
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
      });
    });

    return services;
  }
}
