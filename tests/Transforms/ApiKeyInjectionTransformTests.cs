using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using ApexGateway.Transforms;
using Yarp.ReverseProxy.Transforms;

namespace ApexGateway.Tests.Transforms;

public sealed class ApiKeyInjectionTransformTests
{
  [Fact]
  public async Task ApplyAsync_ShouldInjectAuthorizationHeader()
  {
    var configurationMock = new Mock<IConfiguration>();
    configurationMock.Setup(c => c["OPENAI_API_KEY"]).Returns("sk-test-key");

    var transform = new ApiKeyInjectionTransform(configurationMock.Object);

    var httpContext = new DefaultHttpContext();
    var proxyRequest = new HttpRequestMessage();
    var transformContext = new RequestTransformContext
    {
      HttpContext = httpContext,
      ProxyRequest = proxyRequest
    };

    await transform.ApplyAsync(transformContext);

    proxyRequest.Headers.Authorization.Should().NotBeNull();
    proxyRequest.Headers.Authorization!.Scheme.Should().Be("Bearer");
    proxyRequest.Headers.Authorization!.Parameter.Should().Be("sk-test-key");
  }

  [Fact]
  public void Constructor_ShouldThrowInvalidOperationException_WhenApiKeyIsMissing()
  {
    var configurationMock = new Mock<IConfiguration>();
    configurationMock.Setup(c => c["OPENAI_API_KEY"]).Returns((string?)null);

    Action act = () => new ApiKeyInjectionTransform(configurationMock.Object);

    act.Should().Throw<InvalidOperationException>();
  }
}
