using System.Net;
using System.Net.Http;
using System.Threading;

namespace Cabazure.Client;

/// <summary>
/// Represents a streaming response from an endpoint.
/// </summary>
/// <param name="IsSuccess">Boolean value that indicates if the HTTP response was successful.</param>
/// <param name="StatusCode">The status code of the HTTP response.</param>
/// <param name="Content">The raw content, in case the response was not successful.</param>
/// <param name="ContentObject">The deserialized response, in case the response was not successful.</param>
/// <param name="OkContent">The response stream. This is only set in case the response was successful.</param>
/// <param name="ContentType">The content type of the response, read from the Content-Type header.</param>
/// <param name="Headers">The response headers.</param>
public record StreamResponse(
    bool IsSuccess,
    HttpStatusCode StatusCode,
    string? Content,
    object? ContentObject,
    Stream? OkContent,
    string? ContentType,
    IReadOnlyDictionary<string, IEnumerable<string>> Headers)
    : EndpointResponse(
        IsSuccess,
        StatusCode,
        Content,
        ContentObject,
        Headers),
    IDisposable
{
    private readonly HttpResponseMessage? response;
    private readonly CancellationTokenSource? timeoutCts;

    public StreamResponse(
        HttpResponseMessage? response,
        bool isSuccess,
        HttpStatusCode statusCode,
        string? content,
        object? contentObject,
        Stream? okContent,
        string? contentType,
        IReadOnlyDictionary<string, IEnumerable<string>> headers)
        : this(
            response,
            isSuccess,
            statusCode,
            content,
            contentObject,
            okContent,
            contentType,
            headers,
            timeoutCts: null)
    {
    }

    public StreamResponse(
        HttpResponseMessage? response,
        bool isSuccess,
        HttpStatusCode statusCode,
        string? content,
        object? contentObject,
        Stream? okContent,
        string? contentType,
        IReadOnlyDictionary<string, IEnumerable<string>> headers,
        CancellationTokenSource? timeoutCts)
        : this(
            isSuccess,
            statusCode,
            content,
            contentObject,
            okContent,
            contentType,
            headers)
    {
        this.response = response;
        this.timeoutCts = timeoutCts;
    }

    public void Dispose()
    {
        response?.Dispose();
        timeoutCts?.Dispose();
        GC.SuppressFinalize(this);
    }
}
