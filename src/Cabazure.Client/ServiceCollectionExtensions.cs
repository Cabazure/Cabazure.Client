using System.Text.Json;
using Cabazure.Client.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Cabazure.Client;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddClient(
        this IServiceCollection services,
        string name,
        Action<IHttpClientBuilder>? builder = null,
        Action<JsonSerializerOptions>? jsonOptions = null)
    {
        var clientBuilder = services.AddHttpClient(name);
        builder?.Invoke(clientBuilder);

        services
            .AddOptions<JsonSerializerOptions>(name)
            .Configure(jsonOptions ?? new Action<JsonSerializerOptions>(_ => { }));

        services.TryAddSingleton<IClientSerializer, ClientSerializer>();
        services.TryAddSingleton<IMessageRequestFactory, MessageRequestFactory>();

        return services;
    }

    public static IServiceCollection AddClientJsonSerializerOptions(
        this IServiceCollection services,
        string name,
        Action<JsonSerializerOptions>? jsonOptions = null)
    {
        services
            .AddOptions<JsonSerializerOptions>(name)
            .Configure(jsonOptions ?? new Action<JsonSerializerOptions>(_ => { }));

        return services;
    }


}
