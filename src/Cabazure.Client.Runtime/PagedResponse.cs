using System.Net;

namespace Cabazure.Client;

/// <summary>
/// Represents a pagination response from an endpoint.
/// </summary>
/// <typeparam name="T">The contract type for a successful response</typeparam>
/// <param name="IsSuccess">Boolean value that indicates if the HTTP response was successful.</param>
/// <param name="StatusCode">The status code of the HTTP response.</param>
/// <param name="Content">The raw json response.</param>
/// <param name="ContentObject">The deserialized response.</param>
/// <param name="OkContent">The deserialized <typeparamref name="T"/> response. This is only set in case the response was successful.</param>
/// <param name="ContinuationToken">The continuation token for the next page.</param>
/// <param name="Headers">The response headers.</param>
public record PagedResponse<T>(
    bool IsSuccess,
    HttpStatusCode StatusCode,
    string? Content,
    object? ContentObject,
    T? OkContent,
    string? ContinuationToken,
    IReadOnlyDictionary<string, IEnumerable<string>> Headers)
    : EndpointResponse(
        IsSuccess,
        StatusCode,
        Content,
        ContentObject,
        Headers)
    where T : class
{
    private const string HeaderContinuation = "x-continuation";

    public PagedResponse(
        EndpointResponse response)
        : this(
            response.IsSuccess,
            response.StatusCode,
            response.Content,
            response.ContentObject,
            response.ContentObject as T,
            GetContinuationToken(response.Headers),
            response.Headers)
    {
        ContentObject = response.ContentObject;
    }

    private static string? GetContinuationToken(
        IReadOnlyDictionary<string, IEnumerable<string>> headers)
        => (headers != null && headers.TryGetValue(HeaderContinuation, out var value))
            ? value.FirstOrDefault()
            : null;
}