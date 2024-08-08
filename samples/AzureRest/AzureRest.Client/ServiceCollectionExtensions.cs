using System;
using AzureRest.Client;
using AzureRest.Contracts.Serialization;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAzureRestClient(
            this IServiceCollection services,
            Action<AzureRestClientOptions>? clientOptions = null)
            => services.AddCabazureClient(
                "azure-rest-client",
                j => JsonSerializerOptionsFactory.Configure(j),
                clientOptions);
    }
}
