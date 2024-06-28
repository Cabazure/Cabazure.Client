using Azure.Identity;
using Microsoft.Extensions.Options;
using Microsoft.Graph.Client;

namespace Microsoft.Graph.TestApp.Options;

internal class ConfigureGraphClientOptions
    : IConfigureOptions<GraphClientOptions>
{
    public void Configure(GraphClientOptions options)
    {
        options.Credential = new DefaultAzureCredential();
    }
}
