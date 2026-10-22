using System.Net;
using Microsoft.Extensions.DependencyInjection;

namespace Cabazure.Client.IntegrationTests;

public class InterfaceFacadeTests
{
    private const string ClientName = "FacadeClient";

    [ClientEndpoint(ClientName)]
    public interface IRefreshEndpoint
    {
        public Task<EndpointResponse> ExecuteAsync(
            string id,
            bool refresh,
            ClientRequestOptions options,
            CancellationToken cancellationToken)
            => SendAsync(id, refresh ? "no-cache" : null, options, cancellationToken);

        [Get("/items/{id}")]
        internal Task<EndpointResponse> SendAsync(
            [Path] string id,
            [Header("Cache-Control")] string? cacheControl,
            ClientRequestOptions options,
            CancellationToken cancellationToken);
    }

    [ClientEndpoint(ClientName)]
    public interface IDefaultEndpoint
    {
        public string Describe() => "items";
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Should_Map_Refresh_To_Header(bool refresh)
    {
        using var handler = new CaptureHandler();
        var services = new ServiceCollection();
        services.AddCabazureClient(ClientName, null, b => b
            .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://example.test"))
            .ConfigurePrimaryHttpMessageHandler(() => handler));
        using var provider = services.BuildServiceProvider();
        var endpoint = provider.GetRequiredService<IRefreshEndpoint>();

        var response = await endpoint.ExecuteAsync(
            "123",
            refresh,
            new ClientRequestOptions { CorrelationId = "correlation-123" },
            TestContext.Current.CancellationToken);

        response.IsSuccess.Should().BeTrue();
        handler.RequestUri.Should().Be(new Uri("https://example.test/items/123"));
        handler.CacheControl.Should().Be(refresh ? "no-cache" : null);
        handler.CorrelationId.Should().Be("correlation-123");
    }

    [Fact]
    public void Should_Register_DefaultOnly_Interface()
    {
        var services = new ServiceCollection();
        services.AddCabazureClient(ClientName, null, _ => { });
        using var provider = services.BuildServiceProvider();

        provider.GetRequiredService<IDefaultEndpoint>()
            .Describe()
            .Should()
            .Be("items");
    }

    private sealed class CaptureHandler : HttpMessageHandler
    {
        public Uri? RequestUri { get; private set; }

        public string? CacheControl { get; private set; }

        public string? CorrelationId { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestUri = request.RequestUri;
            CacheControl = request.Headers.CacheControl?.ToString();
            CorrelationId = request.Headers.GetValues("x-correlation-id").Single();

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(string.Empty),
            });
        }
    }
}
