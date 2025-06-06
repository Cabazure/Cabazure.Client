namespace Cabazure.Client;

public interface IRequestOptions
{
    /// <summary>
    /// The timeout for the request.
    /// </summary>
    TimeSpan? Timeout { get; set; }

    /// <summary>
    /// Allows the options implementation to configure the HTTP request message.
    /// </summary>
    /// <param name="request">The http request being sent.</param>
    void ConfigureHttpRequest(HttpRequestMessage request);
}
