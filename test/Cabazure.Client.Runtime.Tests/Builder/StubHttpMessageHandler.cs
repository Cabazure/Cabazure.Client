namespace Cabazure.Client.Runtime.Tests.Builder;

/// <summary>
/// A minimal <see cref="HttpMessageHandler"/> stub that invokes a caller-supplied delegate,
/// used to exercise real <see cref="HttpClient"/> send behavior (including cancellation and
/// completion-option handling) without hitting the network.
/// </summary>
internal sealed class StubHttpMessageHandler(
    Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler)
    : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
        => handler(request, cancellationToken);
}
