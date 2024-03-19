﻿//HintName: ServiceCollectionExtensions.g.cs
using System.Text.Json;
using Cabazure.Client.Builder;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static partial class ServiceCollectionExtensions
{
    static partial void ClientInitialization(
        this IServiceCollection services,
        Action<IHttpClientBuilder> builder,
        Action<JsonSerializerOptions> jsonOptions)
    {
        var clientBuilder = services.AddHttpClient("ClientName");
        builder?.Invoke(clientBuilder);

        services
            .AddOptions<JsonSerializerOptions>("ClientName")
            .Configure(jsonOptions ?? new Action<JsonSerializerOptions>(_ => { }));

        services.TryAddSingleton<IClientSerializer, ClientSerializer>();
        services.TryAddSingleton<IMessageRequestFactory, MessageRequestFactory>();

        services.AddSingleton<ITestEndpoint, TestEndpoint>();
    }
}
