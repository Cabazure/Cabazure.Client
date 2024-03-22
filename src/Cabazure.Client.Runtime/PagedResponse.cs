using System.Net;

namespace Cabazure.Client;

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