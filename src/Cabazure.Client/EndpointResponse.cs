using System.Net;

namespace Cabazure.Client;

public record EndpointResponse<T>(
    bool IsSuccess,
    HttpStatusCode StatusCode,
    string? Content,
    object? ContentObject,
    T? OkContent,
    IDictionary<string, IEnumerable<string>> Headers)
    : EndpointResponse(
        IsSuccess,
        StatusCode,
        Content,
        ContentObject,
        Headers)
    where T : class
{
    public EndpointResponse(
            EndpointResponse response)
            : this(
                  response.IsSuccess,
                  response.StatusCode,
                  response.Content,
                  response.ContentObject,
                  response.ContentObject as T,
                  response.Headers)
    {
        ContentObject = response.ContentObject;
    }
}

public record EndpointResponse(
    bool IsSuccess,
    HttpStatusCode StatusCode,
    string? Content,
    object? ContentObject,
    IDictionary<string, IEnumerable<string>> Headers)
{
    public bool IsOk
        => StatusCode == HttpStatusCode.OK;

    public bool IsNotFound
        => StatusCode == HttpStatusCode.NotFound;

    public bool IsTimeout
        => StatusCode == HttpStatusCode.GatewayTimeout;

    public bool IsBadRequest
        => StatusCode == HttpStatusCode.BadRequest;
}
