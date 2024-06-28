using Azure.Core;
using Cabazure.Client;

namespace Microsoft.Graph.Client;

public class GraphClientOptions : ICabazureAuthClientOptions
{
    public TokenCredential? Credential { get; set; }

    Uri ICabazureClientOptions.GetBaseAddress()
        => new("https://graph.microsoft.com/");

    string ICabazureAuthClientOptions.GetScope()
        => "https://graph.microsoft.com/.default";

    TokenCredential ICabazureAuthClientOptions.GetCredential()
        => Credential
        ?? throw new InvalidOperationException(
            "No Credential configured for GraphClientOptions");

}
