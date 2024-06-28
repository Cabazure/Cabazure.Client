using Azure.Core;

namespace Cabazure.Client;

/// <summary>
/// Options for a client that requires authentication.
/// </summary>
public interface ICabazureAuthClientOptions : ICabazureClientOptions
{
    /// <summary>
    /// The scope to use for authorization token.
    /// </summary>
    /// <returns>The authorization scope.</returns>
    string GetScope();

    /// <summary>
    /// The credential to use for authentication.
    /// </summary>
    /// <returns>The credential to use for authentication.</returns>
    TokenCredential GetCredential();
}