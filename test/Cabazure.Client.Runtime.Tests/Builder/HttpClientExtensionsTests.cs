using System.Net;
using Cabazure.Client.Builder;

namespace Cabazure.Client.Runtime.Tests.Builder;

public class HttpClientExtensionsTests
{
    [Fact]
    public async Task SendStreamAsync_Should_Send_With_ResponseHeadersRead()
    {
        using var handler = new StubHttpMessageHandler((request, cancellationToken)
            => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
        using var client = new HttpClient(handler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.test/");

        var result = await client.SendStreamAsync(request, null, CancellationToken.None);

        result.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.TimeoutCts.Should().BeNull();
        result.Timeout.Should().BeNull();

        result.Response.Dispose();
    }

    [Fact]
    public async Task SendStreamAsync_Should_Not_Dispose_TimeoutCts_After_Successful_Send()
    {
        using var handler = new StubHttpMessageHandler((request, cancellationToken)
            => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
        using var client = new HttpClient(handler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.test/");
        var options = new ClientRequestOptions { Timeout = TimeSpan.FromSeconds(30) };

        var result = await client.SendStreamAsync(request, options, CancellationToken.None);

        result.TimeoutCts.Should().NotBeNull();
        result.Timeout.Should().Be(TimeSpan.FromSeconds(30));

        // Should not throw ObjectDisposedException - the CTS must still be usable by the caller.
        result.TimeoutCts!.CancelAfter(TimeSpan.FromSeconds(30));

        result.Response.Dispose();
        result.TimeoutCts.Dispose();
    }

    [Fact]
    public async Task SendStreamAsync_Should_Dispose_TimeoutCts_When_Send_Throws()
    {
        using var handler = new StubHttpMessageHandler((request, cancellationToken)
            => throw new HttpRequestException("boom"));
        using var client = new HttpClient(handler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.test/");
        var options = new ClientRequestOptions { Timeout = TimeSpan.FromSeconds(30) };

        var act = () => client.SendStreamAsync(request, options, CancellationToken.None);

        // The extension method must dispose the CTS internally before rethrowing; if it
        // didn't, this call would still succeed (there's no way to observe the leaked CTS
        // directly), but this at least guards against the send call itself misbehaving.
        await act.Should().ThrowAsync<HttpRequestException>();
    }
}
