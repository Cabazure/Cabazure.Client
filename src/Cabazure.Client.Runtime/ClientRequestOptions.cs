namespace Cabazure.Client;

public class ClientRequestOptions
{
    public string? CorrelationId { get; set; }

    public string? OnBehalfOf { get; set; }

    public TimeSpan Timeout { get; set; }
}

public class ClientPaginationOptions : ClientRequestOptions
{
    public int? MaxItemCount { get; set; }

    public string? ContinuationToken { get; set; }
}
