namespace Cabazure.Client;

public interface IRequestOptions
{
    /// <summary>
    /// The timeout for the request.
    /// </summary>
    TimeSpan? Timeout { get; set; }

    /// <summary>
    /// Configures the HTTP request after endpoint parameter bindings and before the HTTP handler pipeline.
    /// </summary>
    /// <param name="request">The http request being sent.</param>
    void ConfigureHttpRequest(HttpRequestMessage request);
}
