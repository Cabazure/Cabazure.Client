using System.Net;

namespace Cabazure.Client;

/// <summary>
/// Represents a response from an endpoint.
/// </summary>
/// <typeparam name="T">The contract type for a successful response</typeparam>
/// <param name="IsSuccess">Boolean value that indicates if the HTTP response was successful.</param>
/// <param name="StatusCode">The status code of the HTTP response.</param>
/// <param name="Content">The raw json response.</param>
/// <param name="ContentObject">The deserialized response.</param>
/// <param name="OkContent">The deserialized <typeparamref name="T"/> response. This is only set in case the response was successful.</param>
/// <param name="Headers">The response headers.</param>
public record EndpointResponse<T>(
    bool IsSuccess,
    HttpStatusCode StatusCode,
    string? Content,
    object? ContentObject,
    T? OkContent,
    IReadOnlyDictionary<string, IEnumerable<string>> Headers)
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

/// <summary>
/// Represents a response from an endpoint.
/// </summary>
/// <param name="IsSuccess">Boolean value that indicates if the HTTP response was successful.</param>
/// <param name="StatusCode">The status code of the HTTP response.</param>
/// <param name="Content">The raw json response.</param>
/// <param name="ContentObject">The deserialized response.</param>
/// <param name="Headers">The response headers.</param>
public record EndpointResponse(
    bool IsSuccess,
    HttpStatusCode StatusCode,
    string? Content,
    object? ContentObject,
    IReadOnlyDictionary<string, IEnumerable<string>> Headers)
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
