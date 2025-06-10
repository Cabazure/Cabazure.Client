using Azure.Core;

namespace Cabazure.Client
{
    /// <summary>
    /// Options for a client that requires authentication.
    /// </summary>
    public interface ICabazureAuthClientOptions : ICabazureClientOptions
    {
        /// <summary>
        /// The scopes to use for authorization token.
        /// </summary>
        /// <returns>The authorization scopes.</returns>
        string[] GetScopes();

        /// <summary>
        /// The credential to use for authentication.
        /// </summary>
        /// <returns>The credential to use for authentication.</returns>
        TokenCredential GetCredential();
    }
}
