using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Cabazure.Client.Builder
{
    public static class HttpClientExtensions
    {
        public static HttpClientWithOptions WithRequestOptions(
            this HttpClient httpClient,
            IRequestOptions? requestOptions)
        {
            return new HttpClientWithOptions(httpClient, requestOptions);
        }
    }

    public class HttpClientWithOptions
    {
        private readonly HttpClient client;
        private readonly IRequestOptions? options;

        public HttpClientWithOptions(HttpClient client, IRequestOptions? options)
        {
            this.client = client;
            this.options = options;
        }

        public async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            CancellationTokenSource? timeoutCts = null;
            CancellationToken effectiveCt = cancellationToken;

            if (options is { Timeout: { Ticks: > 0 } timeout })
            {
                timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeoutCts.CancelAfter(timeout);
                effectiveCt = timeoutCts.Token;
            }

            try
            {
                return await client.SendAsync(request, effectiveCt).ConfigureAwait(false);
            }
            finally
            {
                timeoutCts?.Dispose();
            }
        }
    }
}
