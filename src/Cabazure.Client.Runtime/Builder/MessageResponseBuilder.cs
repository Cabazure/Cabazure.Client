using System.Net;

namespace Cabazure.Client.Builder
{
    public delegate object? ContentSerializerFunction(string content);

    public class MessageResponseBuilder : IMessageResponseBuilder
    {
        private static readonly EndpointResponse EmptyResponse = new(
            false,
            HttpStatusCode.InternalServerError,
            string.Empty,
            null,
            new Dictionary<string, IEnumerable<string>>());

        private static readonly StreamResponse EmptyStreamResponse = new(
            null,
            false,
            HttpStatusCode.InternalServerError,
            string.Empty,
            null,
            null,
            null,
            new Dictionary<string, IEnumerable<string>>());

        private readonly Dictionary<HttpStatusCode, ContentSerializerFunction> responseSerializers = [];
        private readonly Dictionary<HttpStatusCode, bool> responseCodes = [];
        private readonly HttpResponseMessage? response;
        private readonly IClientSerializer serializer;
        private readonly string clientName;
        private CancellationTokenSource? streamTimeoutCts;
        private TimeSpan? streamTimeout;

        public MessageResponseBuilder(
            HttpResponseMessage? response,
            IClientSerializer serializer,
            string clientName)
        {
            this.response = response;
            this.serializer = serializer;
            this.clientName = clientName;
        }

        public IMessageResponseBuilder AddErrorResponse(HttpStatusCode statusCode)
            => AddEmptyResponse(statusCode, false);

        public IMessageResponseBuilder AddErrorResponse<TResponseContent>(HttpStatusCode statusCode)
            => AddTypedResponse<TResponseContent>(statusCode, false);

        public IMessageResponseBuilder AddSuccessResponse(HttpStatusCode statusCode)
            => AddEmptyResponse(statusCode, true);

        public IMessageResponseBuilder AddSuccessResponse<TResponseContent>(HttpStatusCode statusCode)
            => AddTypedResponse<TResponseContent>(statusCode, true);

        public IMessageResponseBuilder WithStreamTimeout(CancellationTokenSource? timeoutCts, TimeSpan? timeout)
        {
            streamTimeoutCts = timeoutCts;
            streamTimeout = timeout;

            return this;
        }

        public Task<EndpointResponse> GetAsync(CancellationToken cancellationToken)
            => GetAsync(r => r, cancellationToken);

        public async Task<TResult> GetAsync<TResult>(Func<EndpointResponse, TResult> factory, CancellationToken cancellationToken)
        {
            if (response is null)
            {
                return factory(EmptyResponse);
            }

#if NETSTANDARD2_0 || NETSTANDARD2_1 || NETCOREAPP2_0 || NETCOREAPP2_1 || NETCOREAPP2_2 || NETCOREAPP3_0 || NETCOREAPP3_1
            var content = await response.Content
                .ReadAsStringAsync()
                .ConfigureAwait(false);
#else
            var content = await response.Content
                .ReadAsStringAsync(cancellationToken)
                .ConfigureAwait(false);
#endif

            return factory(
                new EndpointResponse(
                    IsSuccessStatus(response),
                    response.StatusCode,
                    content,
                    GetSerializer(response.StatusCode)?.Invoke(content),
                    GetHeaders(response)));
        }

        public async Task<StreamResponse> GetStreamAsync(CancellationToken cancellationToken)
        {
            if (response is null)
            {
                // Defensive: shouldn't normally happen (a null response means the send
                // never produced a stream to time out), but avoid leaking the CTS if it does.
                streamTimeoutCts?.Dispose();
                return EmptyStreamResponse;
            }

            try
            {
                var isSuccess = IsSuccessStatus(response);
                if (isSuccess)
                {
#if NETSTANDARD2_0 || NETSTANDARD2_1 || NETCOREAPP2_0 || NETCOREAPP2_1 || NETCOREAPP2_2 || NETCOREAPP3_0 || NETCOREAPP3_1
                    var stream = await response.Content
                        .ReadAsStreamAsync()
                        .ConfigureAwait(false);
#else
                    var stream = await response.Content
                        .ReadAsStreamAsync(cancellationToken)
                        .ConfigureAwait(false);
#endif

                    if (streamTimeoutCts is not null && streamTimeout is not null)
                    {
                        stream = new SlidingTimeoutStream(stream, streamTimeoutCts, streamTimeout.Value);
                    }

                    return new StreamResponse(
                        response,
                        true,
                        response.StatusCode,
                        null,
                        null,
                        stream,
                        response.Content.Headers.ContentType?.ToString(),
                        GetHeaders(response),
                        streamTimeoutCts);
                }

#if NETSTANDARD2_0 || NETSTANDARD2_1 || NETCOREAPP2_0 || NETCOREAPP2_1 || NETCOREAPP2_2 || NETCOREAPP3_0 || NETCOREAPP3_1
                var content = await response.Content
                    .ReadAsStringAsync()
                    .ConfigureAwait(false);
#else
                var content = await response.Content
                    .ReadAsStringAsync(cancellationToken)
                    .ConfigureAwait(false);
#endif

                // No live stream is returned on this path, so the timeout CTS is no longer
                // needed once the (already fully read) error content has been captured.
                streamTimeoutCts?.Dispose();

                return new StreamResponse(
                    response,
                    false,
                    response.StatusCode,
                    content,
                    GetSerializer(response.StatusCode)?.Invoke(content),
                    null,
                    response.Content.Headers.ContentType?.ToString(),
                    GetHeaders(response));
            }
            catch
            {
                // GetStreamAsync transfers ownership of the HttpResponseMessage to the
                // returned StreamResponse for disposal by the caller. If reading the
                // content fails before that handoff happens, dispose it here instead.
                response.Dispose();
                streamTimeoutCts?.Dispose();
                throw;
            }
        }

        private bool IsSuccessStatus(HttpResponseMessage responseMessage)
            => responseCodes.TryGetValue(responseMessage.StatusCode, out var isSuccess)
                ? isSuccess
                : responseMessage.IsSuccessStatusCode;

        private ContentSerializerFunction? GetSerializer(HttpStatusCode statusCode)
            => responseSerializers.TryGetValue(statusCode, out var @delegate)
             ? @delegate
             : null;

        private static Dictionary<string, IEnumerable<string>> GetHeaders(HttpResponseMessage responseMessage)
        {
            var headers = responseMessage.Headers.ToDictionary(h => h.Key, h => h.Value);
            if (responseMessage.Content?.Headers is not null)
            {
                foreach (var item_ in responseMessage.Content.Headers)
                {
                    headers[item_.Key] = item_.Value;
                }
            }

            return headers;
        }

        private MessageResponseBuilder AddEmptyResponse(HttpStatusCode statusCode, bool isSuccess)
        {
            responseSerializers[statusCode] = content => null;
            responseCodes[statusCode] = isSuccess;

            return this;
        }

        private MessageResponseBuilder AddTypedResponse<T>(HttpStatusCode statusCode, bool isSuccess)
        {
            responseSerializers[statusCode] = content => serializer.Deserialize<T>(clientName, content);
            responseCodes[statusCode] = isSuccess;

            return this;
        }
    }
}
