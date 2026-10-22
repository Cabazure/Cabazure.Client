using System.Net;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace Cabazure.Client.IntegrationTests;

public class ContentHeaderTests
{
    private const string ClientName = "ContentHeaderClient";

    [ClientEndpoint(ClientName)]
    public interface IContentHeaderEndpoint
    {
        [Post("/items")]
        Task<EndpointResponse> ExecuteAsync(
            [Header("content-type")] string? contentType,
            [Header("Content-Language")] string? language,
            [Body] string body,
            CancellationToken cancellationToken);

        public Task<StreamResponse> DownloadAsync(
            string? contentType,
            string? language,
            string body,
            CancellationToken cancellationToken)
            => SendDownloadAsync(contentType, language, body, cancellationToken);

        [Post("/items")]
        internal Task<StreamResponse> SendDownloadAsync(
            [Header("Content-Type")] string? contentType,
            [Header("Content-Language")] string? language,
            [Body] string body,
            CancellationToken cancellationToken);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task Should_Route_ContentHeaders_And_Preserve_JsonBody(bool streaming, bool includeHeaders)
    {
        using var handler = new ContentHeaderHandler(includeHeaders);
        var services = new ServiceCollection();
        services.AddCabazureClient(
            ClientName,
            j => j.TypeInfoResolver = ContentHeaderJsonContext.Default,
            b => b
                .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://example.test"))
                .ConfigurePrimaryHttpMessageHandler(() => handler));
        using var provider = services.BuildServiceProvider();
        var endpoint = provider.GetRequiredService<IContentHeaderEndpoint>();
        var contentType = includeHeaders ? "application/custom+json" : null;
        var language = includeHeaders ? "en" : null;
        var cancellationToken = TestContext.Current.CancellationToken;

        if (streaming)
        {
            using var response = await endpoint.DownloadAsync(contentType, language, "payload", cancellationToken);
            response.IsSuccess.Should().BeTrue();
            using var reader = new StreamReader(response.OkContent!);
            (await reader.ReadToEndAsync(cancellationToken)).Should().Be("result");
        }
        else
        {
            var response = await endpoint.ExecuteAsync(contentType, language, "payload", cancellationToken);
            response.IsSuccess.Should().BeTrue();
        }

        handler.SendCount.Should().Be(1);
    }

    private sealed class ContentHeaderHandler(bool includeHeaders) : HttpMessageHandler
    {
        public int SendCount { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            request.Method.Should().Be(HttpMethod.Post);
            request.Content.Should().NotBeNull();
            request.Content!.Headers.ContentType!.MediaType.Should().Be(
                includeHeaders ? "application/custom+json" : "application/json");
            request.Content.Headers.ContentLanguage.Should().Equal(
                includeHeaders ? ["en"] : Array.Empty<string>());
            request.Headers.Should().NotContain(h => h.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase)
                || h.Key.Equals("Content-Language", StringComparison.OrdinalIgnoreCase));
            (await request.Content.ReadAsStringAsync(cancellationToken)).Should().Be("\"payload\"");
            SendCount++;

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("result"),
            };
        }
    }
}

[JsonSerializable(typeof(string))]
internal partial class ContentHeaderJsonContext : JsonSerializerContext;
