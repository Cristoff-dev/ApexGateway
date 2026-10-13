using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using StackExchange.Redis;
using ApexGateway.Middlewares;

namespace ApexGateway.Tests.Middlewares;

public sealed class CacheResponseMiddlewareTests
{
  [Fact]
  public async Task InvokeAsync_ShouldReturnCachedResponse_WhenKeyExists()
  {
    var redisMock = new Mock<IConnectionMultiplexer>();
    var dbMock = new Mock<IDatabase>();
    var cachedJson = new RedisValue("{\"status\":\"success\"}");

    dbMock.Setup(db => db.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
          .ReturnsAsync(cachedJson);

    redisMock.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
             .Returns(dbMock.Object);

    var context = new DefaultHttpContext();
    context.Request.Method = HttpMethods.Get;
    context.Request.Path = "/api/v1/ai/data";
    context.Request.QueryString = new QueryString("?param=1");

    var middleware = new CacheResponseMiddleware(innerHttpContext => Task.CompletedTask);

    await middleware.InvokeAsync(context, redisMock.Object);

    context.Response.Headers["X-Cache-Status"].ToString().Should().Be("HIT");
    context.Response.ContentType.Should().Be("application/json");
  }

  [Fact]
  public async Task InvokeAsync_ShouldCallNext_WhenCacheMisses()
  {
    var redisMock = new Mock<IConnectionMultiplexer>();
    var dbMock = new Mock<IDatabase>();

    dbMock.Setup(db => db.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
          .ReturnsAsync(RedisValue.Null);

    redisMock.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
             .Returns(dbMock.Object);

    var context = new DefaultHttpContext();
    context.Request.Method = HttpMethods.Get;
    context.Request.Path = "/api/v1/ai/data";

    var nextCalled = false;
    var middleware = new CacheResponseMiddleware(innerHttpContext =>
    {
      nextCalled = true;
      return Task.CompletedTask;
    });

    await middleware.InvokeAsync(context, redisMock.Object);

    context.Response.Headers["X-Cache-Status"].ToString().Should().Be("MISS");
    nextCalled.Should().BeTrue();
  }

  [Fact]
  public async Task InvokeAsync_ShouldBypassCache_WhenMethodIsNotGet()
  {
    var redisMock = new Mock<IConnectionMultiplexer>();
    var context = new DefaultHttpContext();
    context.Request.Method = HttpMethods.Post;

    var nextCalled = false;
    var middleware = new CacheResponseMiddleware(innerHttpContext =>
    {
      nextCalled = true;
      return Task.CompletedTask;
    });

    await middleware.InvokeAsync(context, redisMock.Object);

    context.Response.Headers.ContainsKey("X-Cache-Status").Should().BeFalse();
    nextCalled.Should().BeTrue();
    redisMock.Verify(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>()), Times.Never);
  }
}
