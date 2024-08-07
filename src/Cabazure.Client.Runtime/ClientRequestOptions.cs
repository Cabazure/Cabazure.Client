using Microsoft.Extensions.Primitives;

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

    IDictionary<string, StringValues> IRequestOptions.GetHeaders()
    {
        var headers = new Dictionary<string, StringValues>();
        AppendHeaders(headers);

        return headers;
    }

    protected virtual void AppendHeaders(
        IDictionary<string, StringValues> headers)
    { 
        if (CorrelationId is { } id)
        {
            headers[HeaderCorrelationId] = id;
        }
    }
}
