using Microsoft.Extensions.Primitives;

namespace Cabazure.Client;

public interface IRequestOptions
{
    /// <summary>
    /// The timeout for the request.
    /// </summary>
    TimeSpan? Timeout { get; set; }

    /// <summary>
    /// Gets the headers to be sent with the request.
    /// </summary>
    /// <returns>Dictionary of header names and header values.</returns>
    IDictionary<string, StringValues> GetHeaders();
}
