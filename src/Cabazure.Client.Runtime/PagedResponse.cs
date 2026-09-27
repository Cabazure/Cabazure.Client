using System.Diagnostics.CodeAnalysis;
using System.Globalization;
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
    private const string HeaderTotalItemCount = "x-total-item-count";

    /// <summary>
    /// The total number of items across all pages, when provided by the response.
    /// </summary>
    public long? TotalCount { get; } = GetTotalCount(Headers);

    [SuppressMessage(
        "Style",
        "IDE1006:Naming Styles",
        Justification = "Parameter names match the positional record properties and preserve consistent named-argument usage.")]
    public PagedResponse(
        bool IsSuccess,
        HttpStatusCode StatusCode,
        string? Content,
        object? ContentObject,
        T? OkContent,
        string? ContinuationToken,
        long? TotalCount,
        IReadOnlyDictionary<string, IEnumerable<string>> Headers)
        : this(
            IsSuccess,
            StatusCode,
            Content,
            ContentObject,
            OkContent,
            ContinuationToken,
            Headers)
    {
        this.TotalCount = TotalCount;
    }

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
    }

    private static string? GetContinuationToken(
        IReadOnlyDictionary<string, IEnumerable<string>> headers)
        => GetFirstHeaderValue(headers, HeaderContinuation);

    private static long? GetTotalCount(
        IReadOnlyDictionary<string, IEnumerable<string>> headers)
        => long.TryParse(
                GetFirstHeaderValue(headers, HeaderTotalItemCount),
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var totalCount)
            ? totalCount
            : null;

    private static string? GetFirstHeaderValue(
        IReadOnlyDictionary<string, IEnumerable<string>> headers,
        string headerName)
    {
        if (headers is null)
        {
            return null;
        }

        if (!headers.TryGetValue(headerName, out var values))
        {
            values = headers
                .FirstOrDefault(header => string.Equals(
                    header.Key,
                    headerName,
                    StringComparison.OrdinalIgnoreCase))
                .Value;
        }

        return values?.FirstOrDefault();
    }
}
