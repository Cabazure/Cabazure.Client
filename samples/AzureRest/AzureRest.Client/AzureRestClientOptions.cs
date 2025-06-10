using System;
using Azure.Core;
using Cabazure.Client;

namespace AzureRest.Client
{
    public class AzureRestClientOptions : ICabazureAuthClientOptions
    {
        public TokenCredential? Credential { get; set; }

        Uri ICabazureClientOptions.GetBaseAddress()
            // Notice the trailing slash, it's important
            // for the client to work correctly with relative paths.
            => new("https://management.azure.com/");

        string[] ICabazureAuthClientOptions.GetScopes()
            // The default scope for Microsoft Graph.
            => new string[] { "https://management.azure.com/.default" };

        TokenCredential ICabazureAuthClientOptions.GetCredential()
            => Credential
            ?? throw new InvalidOperationException(
                "No Credential configured for GraphClientOptions");
    }
}
