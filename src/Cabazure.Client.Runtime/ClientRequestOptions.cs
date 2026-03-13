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

    /// <summary>
    /// The HTTP version to use for the request (e.g. <c>new Version(2, 0)</c> for HTTP/2).
    /// When not set, the HttpClient version negotiation is used.
    /// </summary>
    public Version? HttpVersion { get; set; }

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

        if (HttpVersion is { } version)
        {
            request.Version = version;
        }
    }
}
