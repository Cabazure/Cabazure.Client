namespace Cabazure.Client;

/// <summary>
/// Options for a client request.
/// </summary>
public class ClientRequestOptions
{
    /// <summary>
    /// The correlation id to use for the request, sent via the x-correlation-id header.
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// The name of the user on whose behalf the request is made, sent via the x-on-behalf-of header.
    /// </summary>
    public string? OnBehalfOf { get; set; }

    /// <summary>
    /// The timeout for the request.
    /// </summary>
    public TimeSpan? Timeout { get; set; }
}
