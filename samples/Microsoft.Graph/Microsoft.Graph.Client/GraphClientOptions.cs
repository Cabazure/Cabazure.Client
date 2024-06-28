using Azure.Core;
using Cabazure.Client;

namespace Microsoft.Graph.Client;

public class GraphClientOptions : ICabazureAuthClientOptions
{
    public TokenCredential? Credential { get; set; }

    Uri ICabazureClientOptions.GetBaseAddress()
        // Notice the trailing slash, it's important
        // for the client to work correctly with relative paths.
        => new("https://graph.microsoft.com/v1.0/"); 

    string ICabazureAuthClientOptions.GetScope()
        // The default scope for Microsoft Graph.
        => "https://graph.microsoft.com/.default";

    TokenCredential ICabazureAuthClientOptions.GetCredential()
        => Credential
        ?? throw new InvalidOperationException(
            "No Credential configured for GraphClientOptions");

}
