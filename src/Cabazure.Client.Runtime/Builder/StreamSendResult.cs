namespace Cabazure.Client.Builder
{
    /// <summary>
    /// The result of sending a stream endpoint's request, bundling the response together with
    /// the (still-alive) linked timeout <see cref="CancellationTokenSource"/> so the sliding
    /// timeout can be extended while the response body is streamed to the caller.
    /// </summary>
    /// <param name="Response">The HTTP response, sent with <see cref="HttpCompletionOption.ResponseHeadersRead"/>.</param>
    /// <param name="TimeoutCts">
    /// The linked <see cref="CancellationTokenSource"/> created for <see cref="IRequestOptions.Timeout"/>,
    /// if one was configured. The caller is responsible for disposing this once it is no longer needed.
    /// </param>
    /// <param name="Timeout">The configured timeout, if any, used to reset <paramref name="TimeoutCts"/>'s deadline on each read.</param>
    public sealed record StreamSendResult(
        HttpResponseMessage Response,
        CancellationTokenSource? TimeoutCts,
        TimeSpan? Timeout);
}
