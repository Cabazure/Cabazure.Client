namespace Cabazure.Client;

/// <summary>
/// Options for a client request, that supports pagination.
/// </summary>
public class ClientPaginationOptions : ClientRequestOptions
{
    /// <summary>
    /// The maximum number of items to return, sent via the x-max-item-count header.
    /// </summary>
    public int? MaxItemCount { get; set; }

    /// <summary>
    /// The continuation token to use for fetching the next page of items, sent via the x-continuation-token header.
    /// </summary>
    public string? ContinuationToken { get; set; }
}
