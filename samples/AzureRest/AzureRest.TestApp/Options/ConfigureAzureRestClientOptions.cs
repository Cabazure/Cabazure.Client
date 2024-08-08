using Azure.Identity;
using AzureRest.Client;
using Microsoft.Extensions.Options;

namespace AzureRest.TestApp.Options;

public class ConfigureAzureRestClientOptions
    : IConfigureOptions<AzureRestClientOptions>
{
    public void Configure(AzureRestClientOptions options)
    {
        options.Credential = new DefaultAzureCredential();
    }
}
