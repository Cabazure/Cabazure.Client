using Azure.Core;

namespace Cabazure.Client;

public interface ICabazureAuthClientOptions : ICabazureClientOptions
{
    string GetScope();

    TokenCredential GetCredential();
}