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
        /// Sends the request with an explicit <see cref="HttpCompletionOption"/>, allowing
        /// callers (such as stream endpoints) to opt out of buffering the response content
        /// via <see cref="HttpCompletionOption.ResponseHeadersRead"/>.
        /// </summary>
        public static async Task<HttpResponseMessage> SendAsync(
            this HttpClient httpClient,
            HttpRequestMessage request,
            IRequestOptions? requestOptions,
            HttpCompletionOption completionOption,
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
                return await httpClient.SendAsync(request, completionOption, effectiveCt).ConfigureAwait(false);
            }
            finally
            {
                timeoutCts?.Dispose();
            }
        }
    }
}
