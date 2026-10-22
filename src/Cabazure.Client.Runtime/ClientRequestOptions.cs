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

    /// <summary>
    /// Configures the request after endpoint parameter bindings and before the HTTP handler pipeline.
    /// </summary>
    /// <remarks>
    /// Override for client-specific options and call the base implementation to preserve built-in headers.
    /// Subclasses of PagedRequestOptions should call their base implementation to preserve pagination as well.
    /// </remarks>
    /// <param name="request">The request being sent.</param>
    protected virtual void ConfigureHttpRequest(HttpRequestMessage request)
    {
        if (CorrelationId is { } id)
        {
            request.Headers.TryAddWithoutValidation(HeaderCorrelationId, id);
        }
    }
}
