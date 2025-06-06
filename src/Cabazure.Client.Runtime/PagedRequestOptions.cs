using System.Globalization;

namespace Cabazure.Client;

/// <summary>
/// Options for a client request, that supports pagination.
/// </summary>
public class PagedRequestOptions : ClientRequestOptions
{
    private const string HeaderMaxItemCount = "x-max-item-count";
    private const string HeaderContinuation = "x-continuation";

    /// <summary>
    /// The maximum number of items to return, sent via the x-max-item-count header.
    /// </summary>
    public int? MaxItemCount { get; set; }

    /// <summary>
    /// The continuation token to use for fetching the next page of items, sent via the x-continuation-token header.
    /// </summary>
    public string? ContinuationToken { get; set; }

    protected override void ConfigureHttpRequest(HttpRequestMessage request)
    {
        base.ConfigureHttpRequest(request);

        if (MaxItemCount is { } maxItemCount)
        {
            request.Headers.TryAddWithoutValidation(HeaderMaxItemCount, maxItemCount.ToString(CultureInfo.InvariantCulture));
        }

        if (ContinuationToken is { } continuationToken)
        {
            request.Headers.TryAddWithoutValidation(HeaderContinuation, continuationToken);
        }
    }
}
