namespace Cabazure.Client;

/// <summary>
/// Options for a client request.
/// </summary>
public class ClientRequestOptions : IRequestOptions
{
    private const string HeaderCorrelationId = "x-correlation-id";

    /// <summary>
    /// The correlation id to use for the request, sent via the x-correlation-id header.
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// The timeout for the request.
    /// </summary>
    public TimeSpan? Timeout { get; set; }

    void IRequestOptions.ConfigureHttpRequest(HttpRequestMessage request)
    {
        ConfigureHttpRequest(request);
    }

    protected virtual void ConfigureHttpRequest(HttpRequestMessage request)
    {
        if (CorrelationId is { } id)
        {
            request.Headers.TryAddWithoutValidation(HeaderCorrelationId, id);
        }
    }
}
