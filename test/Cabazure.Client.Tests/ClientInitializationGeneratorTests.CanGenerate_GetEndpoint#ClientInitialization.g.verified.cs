﻿//HintName: ClientInitialization.g.cs
// <auto-generated/>
#nullable enable
using System.Text.Json;
using Azure.Core;
using Cabazure.Client;
using Cabazure.Client.Authentication;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

internal static partial class ClientInitialization
{
    internal static IServiceCollection AddCabazureClient<TOptions>(
        this IServiceCollection services,
        string clientName,
        Action<JsonSerializerOptions>? jsonOptions,
        Action<TOptions>? clientOptions,
        Action<IHttpClientBuilder>? builder = default)
        where TOptions : class, ICabazureClientOptions
    {
        if (clientOptions != null)
        {
            services
                .AddOptions<TOptions>()
                .Configure(clientOptions);
        }

        void ConfigureHttpClient(IServiceProvider services, HttpClient client)
            => client.BaseAddress = services
                .GetRequiredService<IOptions<TOptions>>()
                .Value
                .GetBaseAddress();

        void ConfigureAuthHandler(IList<DelegatingHandler> handlers, IServiceProvider services)
        {
            var options = services
                .GetRequiredService<IOptions<TOptions>>()
                .Value;

            if (options is ICabazureAuthClientOptions authOptions)
            {
                var scope = authOptions.GetScope();
                var credential = authOptions.GetCredential();

                var tokenProvider = new BearerTokenProvider(
                    new TokenRequestContext(new [] { scope }),
                    credential,
                    new DateTimeProvider());

                handlers.Add(new AzureAuthenticationHandler(tokenProvider));
            }
        }

        void BuildHttpClient(IHttpClientBuilder b)
        {
            b.ConfigureHttpClient(ConfigureHttpClient);
            b.ConfigureAdditionalHttpMessageHandlers(ConfigureAuthHandler);
            builder?.Invoke(b);
        }

        return services.AddCabazureClient(
            clientName,
            jsonOptions,
            BuildHttpClient);
    }

    internal static partial IServiceCollection AddCabazureClient(
        this IServiceCollection services,
        string clientName,
        Action<JsonSerializerOptions>? jsonOptions,
        Action<IHttpClientBuilder> builder);
}
#nullable disable