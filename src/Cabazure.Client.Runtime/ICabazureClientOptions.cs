namespace Cabazure.Client;

/// <summary>
/// The base options interface used to describe options that can resolve the BaseAddress of the API.
/// </summary>
public interface ICabazureClientOptions
{
    /// <summary>
    /// The base address of the API.
    /// </summary>
    /// <returns>The base address for the API.</returns>
    /// <remarks>
    /// If the base address contains path segments, use a trailing slash (/) to separate the base address from the relative path segments.
    /// </remarks>
    Uri GetBaseAddress();
}
