namespace Cabazure.Client.Builder
{
    public static class HttpClientExtensions
    {
        public static HttpClient WithRequestOptions(
            this HttpClient httpClient,
            IRequestOptions? requestOptions)
        {
            return httpClient;
        }

        public static async Task<HttpResponseMessage> SendAsync(
            this HttpClient httpClient,
            HttpRequestMessage request,
            IRequestOptions? requestOptions,
            CancellationToken cancellationToken)
        {
            CancellationTokenSource? timeoutCts = null;
            CancellationToken effectiveCt = cancellationToken;

            if (requestOptions is { Timeout: { Ticks: > 0 } timeout })
            {
                timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeoutCts.CancelAfter(timeout);
                effectiveCt = timeoutCts.Token;
            }

            try
            {
                return await httpClient.SendAsync(request, effectiveCt).ConfigureAwait(false);
            }
            finally
            {
                timeoutCts?.Dispose();
            }
        }

        /// <summary>
        /// Sends the request for a stream endpoint, using <see cref="HttpCompletionOption.ResponseHeadersRead"/>
        /// so the response content isn't buffered before the caller reads it as a stream.
        /// </summary>
        /// <remarks>
        /// Unlike the other <c>SendAsync</c> overloads, the linked timeout <see cref="CancellationTokenSource"/>
        /// (if any) is <b>not</b> disposed once headers arrive - it is returned via <see cref="StreamSendResult"/>
        /// so its deadline can be extended (a sliding/idle timeout) while the response body is streamed. The
        /// caller becomes responsible for disposing it, unless the initial send itself fails, in which case it
        /// is disposed here before rethrowing.
        /// </remarks>
        public static async Task<StreamSendResult> SendStreamAsync(
            this HttpClient httpClient,
            HttpRequestMessage request,
            IRequestOptions? requestOptions,
            CancellationToken cancellationToken)
        {
            CancellationTokenSource? timeoutCts = null;
            CancellationToken effectiveCt = cancellationToken;
            TimeSpan? timeout = null;

            if (requestOptions is { Timeout: { Ticks: > 0 } t })
            {
                timeout = t;
                timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeoutCts.CancelAfter(t);
                effectiveCt = timeoutCts.Token;
            }

            try
            {
                var response = await httpClient
                    .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, effectiveCt)
                    .ConfigureAwait(false);

                return new StreamSendResult(response, timeoutCts, timeout);
            }
            catch
            {
                timeoutCts?.Dispose();
                throw;
            }
        }
    }
}
