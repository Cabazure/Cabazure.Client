using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph.Contracts.Serialization;

namespace Microsoft.Graph.Client;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGraphClient(
        this IServiceCollection services,
        Action<GraphClientOptions>? clientOptions = null)
        => services.AddCabazureClient(
            "ms-graph-client",
            j => JsonSerializerOptionsFactory.Configure(j),
            clientOptions);
}
