using System.Collections.Concurrent;
using System.Net;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace Cabazure.Client.IntegrationTests;

public class HandlerCustomizationTests
{
    private const string ClientName = "ContextClient";

    public enum RequestKind
    {
        Buffered,
        Paged,
        Streaming,
    }

    [ClientEndpoint(ClientName)]
    public interface IContextEndpoint
    {
        [Get("/items/{id}")]
        Task<EndpointResponse> GetAsync(
            [Path] string id,
            [Header("x-on-behalf-of")] string? explicitUser,
            UserContextRequestOptions options,
            CancellationToken cancellationToken);

        [Get("/items/{id}")]
        Task<PagedResponse<string[]>> ListAsync(
            [Path] string id,
            [Header("x-on-behalf-of")] string? explicitUser,
            PagedUserContextRequestOptions options,
            CancellationToken cancellationToken);

        [Get("/items/{id}")]
        Task<StreamResponse> DownloadAsync(
            [Path] string id,
            [Header("x-on-behalf-of")] string? explicitUser,
            UserContextRequestOptions options,
            CancellationToken cancellationToken);
    }

    [Theory]
    [InlineData(RequestKind.Buffered, false)]
    [InlineData(RequestKind.Buffered, true)]
    [InlineData(RequestKind.Paged, false)]
    [InlineData(RequestKind.Paged, true)]
    [InlineData(RequestKind.Streaming, false)]
    [InlineData(RequestKind.Streaming, true)]
    public async Task Should_Apply_Context_Through_Registered_Handler(
        RequestKind kind,
        bool separateRegistration)
    {
        using var handler = new CaptureHandler();
        using var provider = CreateServices(handler, separateRegistration).BuildServiceProvider();

        await SendAsync(provider, kind, "123", "user-123", null, TestContext.Current.CancellationToken);

        var request = handler.Requests.Should().ContainSingle().Subject;
        request.UserIds.Should().Equal("user-123");
        request.Configured.Should().BeTrue();
        request.CorrelationId.Should().Be("123");
        request.Path.Should().Be("/items/123");
        request.MaxItemCount.Should().Be(kind == RequestKind.Paged ? "25" : null);
        request.ContinuationToken.Should().Be(kind == RequestKind.Paged ? "page-123" : null);
    }

    [Theory]
    [InlineData(RequestKind.Buffered)]
    [InlineData(RequestKind.Paged)]
    [InlineData(RequestKind.Streaming)]
    public async Task Should_Preserve_Explicit_Header(RequestKind kind)
    {
        using var handler = new CaptureHandler();
        using var provider = CreateServices(handler, true).BuildServiceProvider();

        await SendAsync(provider, kind, "123", "context-user", "explicit-user", TestContext.Current.CancellationToken);

        handler.Requests.Should().ContainSingle()
            .Which.UserIds.Should().Equal("explicit-user");
    }

    [Theory]
    [InlineData(RequestKind.Buffered)]
    [InlineData(RequestKind.Paged)]
    [InlineData(RequestKind.Streaming)]
    public async Task Should_Isolate_Context_Between_Overlapping_Requests(RequestKind kind)
    {
        using var handler = new CaptureHandler(expectedRequests: 3);
        using var provider = CreateServices(handler, true).BuildServiceProvider();
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(10));

        await Task.WhenAll(
            SendAsync(provider, kind, "first", "user-first", null, timeout.Token),
            SendAsync(provider, kind, "second", "user-second", null, timeout.Token),
            SendAsync(provider, kind, "anonymous", null, null, timeout.Token));

        handler.Requests.Should().HaveCount(3);
        foreach (var request in handler.Requests)
        {
            request.Configured.Should().BeTrue();
            request.Path.Should().Be($"/items/{request.CorrelationId}");
            request.UserIds.Should().Equal(request.CorrelationId == "anonymous"
                ? Array.Empty<string>()
                : [$"user-{request.CorrelationId}"]);
            request.MaxItemCount.Should().Be(kind == RequestKind.Paged ? "25" : null);
            request.ContinuationToken.Should().Be(kind == RequestKind.Paged ? $"page-{request.CorrelationId}" : null);
        }
    }

    [Fact]
    public async Task Should_Only_Apply_Handlers_To_Their_NamedClient()
    {
        using var handler = new CaptureHandler();
        using var otherHandler = new CaptureHandler();
        var services = CreateServices(handler, true);
        services.AddHttpClient("OtherClient")
            .ConfigurePrimaryHttpMessageHandler(() => otherHandler);
        using var provider = services.BuildServiceProvider();

        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.test/items/other");
        UserContextMetadata.Set(request, "user-other");
        using var client = provider.GetRequiredService<IHttpClientFactory>().CreateClient("OtherClient");
        using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        response.IsSuccessStatusCode.Should().BeTrue();
        handler.Requests.Should().BeEmpty();
        var captured = otherHandler.Requests.Should().ContainSingle().Subject;
        captured.UserIds.Should().BeEmpty();
        captured.Configured.Should().BeFalse();
    }

    private static ServiceCollection CreateServices(CaptureHandler handler, bool separateRegistration)
    {
        var services = new ServiceCollection();
        services.AddTransient<OnBehalfOfHandler>();
        services.AddCabazureClient(
            ClientName,
            j => j.TypeInfoResolver = HandlerTestJsonContext.Default,
            b =>
            {
                b.ConfigureHttpClient(c => c.BaseAddress = new Uri("https://example.test"))
                    .ConfigurePrimaryHttpMessageHandler(() => handler)
                    .AddHttpMessageHandler(() => new ConfiguredHandler());

                if (!separateRegistration)
                {
                    b.AddHttpMessageHandler<OnBehalfOfHandler>();
                }
            });

        if (separateRegistration)
        {
            services.AddHttpClient(ClientName)
                .AddHttpMessageHandler<OnBehalfOfHandler>();
        }

        return services;
    }

    private static async Task SendAsync(
        IServiceProvider provider,
        RequestKind kind,
        string id,
        string? userId,
        string? explicitUser,
        CancellationToken cancellationToken)
    {
        var endpoint = provider.GetRequiredService<IContextEndpoint>();
        var options = new UserContextRequestOptions
        {
            OnBehalfOf = userId,
            CorrelationId = id,
        };

        switch (kind)
        {
            case RequestKind.Buffered:
                var response = await endpoint.GetAsync(id, explicitUser, options, cancellationToken);
                response.IsSuccess.Should().BeTrue();
                break;

            case RequestKind.Paged:
                var page = await endpoint.ListAsync(id, explicitUser, new PagedUserContextRequestOptions
                {
                    OnBehalfOf = userId,
                    CorrelationId = id,
                    MaxItemCount = 25,
                    ContinuationToken = $"page-{id}",
                }, cancellationToken);
                page.IsSuccess.Should().BeTrue();
                page.OkContent.Should().Equal("item");
                page.ContinuationToken.Should().Be("next-page");
                break;

            case RequestKind.Streaming:
                using (var stream = await endpoint.DownloadAsync(id, explicitUser, options, cancellationToken))
                {
                    stream.IsSuccess.Should().BeTrue();
                    using var reader = new StreamReader(stream.OkContent!);
                    (await reader.ReadToEndAsync(cancellationToken)).Should().Be("[\"item\"]");
                }
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
        }
    }

    public sealed class OnBehalfOfHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (!request.Headers.Contains("x-on-behalf-of")
                && request.Options.TryGetValue(UserContextMetadata.Key, out var userId))
            {
                request.Headers.Add("x-on-behalf-of", userId);
            }

            return base.SendAsync(request, cancellationToken);
        }
    }

    private sealed class ConfiguredHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            request.Headers.Add("x-client-configured", "true");
            return base.SendAsync(request, cancellationToken);
        }
    }

    private sealed record CapturedRequest(
        string? Path,
        string[] UserIds,
        string? CorrelationId,
        string? MaxItemCount,
        string? ContinuationToken,
        bool Configured);

    private sealed class CaptureHandler(int expectedRequests = 1) : HttpMessageHandler
    {
        private readonly TaskCompletionSource allArrived = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private int arrived;

        public ConcurrentQueue<CapturedRequest> Requests { get; } = new();

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Requests.Enqueue(new CapturedRequest(
                request.RequestUri?.AbsolutePath,
                GetValues(request, "x-on-behalf-of"),
                GetValues(request, "x-correlation-id").SingleOrDefault(),
                GetValues(request, "x-max-item-count").SingleOrDefault(),
                GetValues(request, "x-continuation").SingleOrDefault(),
                GetValues(request, "x-client-configured").SingleOrDefault() == "true"));

            if (Interlocked.Increment(ref arrived) == expectedRequests)
            {
                allArrived.SetResult();
            }

            await allArrived.Task.WaitAsync(cancellationToken);

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("[\"item\"]"),
            };
            response.Headers.Add("x-continuation", "next-page");
            return response;
        }

        private static string[] GetValues(HttpRequestMessage request, string name)
            => request.Headers.TryGetValues(name, out var values) ? values.ToArray() : [];
    }
}

public class UserContextRequestOptions : ClientRequestOptions
{
    public string? OnBehalfOf { get; set; }

    protected override void ConfigureHttpRequest(HttpRequestMessage request)
    {
        base.ConfigureHttpRequest(request);
        UserContextMetadata.Set(request, OnBehalfOf);
    }
}

public class PagedUserContextRequestOptions : PagedRequestOptions
{
    public string? OnBehalfOf { get; set; }

    protected override void ConfigureHttpRequest(HttpRequestMessage request)
    {
        base.ConfigureHttpRequest(request);
        UserContextMetadata.Set(request, OnBehalfOf);
    }
}

internal static class UserContextMetadata
{
    internal static readonly HttpRequestOptionsKey<string> Key = new("HandlerCustomization.OnBehalfOf");

    internal static void Set(HttpRequestMessage request, string? userId)
    {
        if (userId is not null)
        {
            request.Options.Set(Key, userId);
        }
    }
}

[JsonSerializable(typeof(string[]))]
internal partial class HandlerTestJsonContext : JsonSerializerContext;
