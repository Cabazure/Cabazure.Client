namespace Cabazure.Client.Builder
{
    public static class HttpClientExtensions
    {
        public static HttpClient WithRequestOptions(
            this HttpClient httpClient,
            IRequestOptions? requestOptions)
        {
            if (requestOptions is { Timeout: { Ticks: > 0 } timeout })
            {
                httpClient.Timeout = timeout;
            }

            return httpClient;
        }
    }
}
