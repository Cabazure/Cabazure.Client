using System.Net;

namespace Cabazure.Client.Builder;

public delegate object? ContentSerializerDelegate(string content);

public class MessageResponseBuilder(
    HttpResponseMessage? response,
    IClientSerializer serializer,
    string clientName)
    : IMessageResponseBuilder
{
    private static readonly EndpointResponse EmptyResponse = new(
        false,
        HttpStatusCode.InternalServerError,
        string.Empty,
        null,
        new Dictionary<string, IEnumerable<string>>());

    private readonly Dictionary<HttpStatusCode, ContentSerializerDelegate> responseSerializers = [];
    private readonly Dictionary<HttpStatusCode, bool> responseCodes = [];

    public IMessageResponseBuilder AddErrorResponse(HttpStatusCode statusCode)
        => AddEmptyResponse(statusCode, false);

    public IMessageResponseBuilder AddErrorResponse<TResponseContent>(HttpStatusCode statusCode)
        => AddTypedResponse<TResponseContent>(statusCode, false);

    public IMessageResponseBuilder AddSuccessResponse(HttpStatusCode statusCode)
        => AddEmptyResponse(statusCode, true);

    public IMessageResponseBuilder AddSuccessResponse<TResponseContent>(HttpStatusCode statusCode)
        => AddTypedResponse<TResponseContent>(statusCode, true);

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
            .ReadAsStringAsync();
#else
        var content = await response.Content
            .ReadAsStringAsync(cancellationToken);
#endif

        return factory(
            new EndpointResponse(
                IsSuccessStatus(response),
                response.StatusCode,
                content,
                GetSerializer(response.StatusCode)?.Invoke(content),
                GetHeaders(response)));
    }

    private bool IsSuccessStatus(HttpResponseMessage responseMessage)
        => responseCodes.TryGetValue(responseMessage.StatusCode, out var isSuccess)
            ? isSuccess
            : responseMessage.IsSuccessStatusCode;

    private ContentSerializerDelegate? GetSerializer(HttpStatusCode statusCode)
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