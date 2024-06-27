﻿//HintName: ClientInitialization.Implementation.g.cs
// <auto-generated/>
#nullable enable
using System.Text.Json;
using Cabazure.Client.Builder;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

internal static partial class ClientInitialization
{
    internal static partial IServiceCollection AddCabazureClient(
        this IServiceCollection services,
        string clientName,
        Action<JsonSerializerOptions>? jsonOptions,
        Action<IHttpClientBuilder> builder)
    {
        if (jsonOptions != null)
        {
            services
                .AddOptions<JsonSerializerOptions>(clientName)
                .Configure(jsonOptions);
        }

        var clientBuilder = services.AddHttpClient(clientName);
        builder.Invoke(clientBuilder);

        services.TryAddSingleton<IClientSerializer, ClientSerializer>();
        services.TryAddSingleton<IMessageRequestFactory, MessageRequestFactory>();
        services.TryAddSingleton<ITestEndpoint, TestEndpoint>();
        return services;
    }
}
#nullable disable
