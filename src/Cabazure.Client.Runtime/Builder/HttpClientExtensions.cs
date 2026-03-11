using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

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
    }
}
