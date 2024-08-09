using System;
using AzureRest.Client;
using AzureRest.Client.Endpoints;
using AzureRest.Contracts.Serialization;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAzureRestClient(
            this IServiceCollection services,
            Action<AzureRestClientOptions>? clientOptions = null)
            => services
                // Custom endpoint needs to be added manually
                .AddSingleton<IListTenantsEndpoint, ListTenantEndpoint>()
                .AddCabazureClient(
                    "azure-rest-client",
                    j => JsonSerializerOptionsFactory.Configure(j),
                    clientOptions);
    }
}
