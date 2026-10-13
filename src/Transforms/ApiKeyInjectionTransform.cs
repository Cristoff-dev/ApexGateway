using System.Net.Http.Headers;
using Yarp.ReverseProxy.Transforms;

namespace ApexGateway.Transforms;

public sealed class ApiKeyInjectionTransform : RequestTransform
{
  private readonly string _apiKey;
  private const string AuthScheme = "Bearer";

  public ApiKeyInjectionTransform(IConfiguration configuration)
  {
    _apiKey = configuration["OPENAI_API_KEY"]
        ?? throw new InvalidOperationException("OPENAI_API_KEY environment variable is not configured.");
  }

  public override ValueTask ApplyAsync(RequestTransformContext context)
  {
    context.ProxyRequest.Headers.Authorization = new AuthenticationHeaderValue(AuthScheme, _apiKey);
    return ValueTask.CompletedTask;
  }
}
