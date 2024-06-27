﻿using System.Text;
using Microsoft.CodeAnalysis;

namespace Cabazure.Client.SourceGenerator;

[Generator]
public class ClientInitializationGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new ClientInitializationSyntaxReceiver());

        context.RegisterForPostInitialization(c =>
            c.AddSource(
            "ClientInitialization.g.cs",
            """
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
                    Action<TOptions>? clientOptions)
                    where TOptions : class, ICabazureClientOptions
                {
                    if (clientOptions != null)
                    {
                        services
                            .AddOptions<TOptions>(clientName)
                            .Configure(clientOptions);
                    }

                    void ConfigureHttpClient(IServiceProvider services, HttpClient client)
                        => client.BaseAddress = services
                            .GetRequiredService<IOptionsMonitor<TOptions>>()
                            .Get(clientName)
                            .GetBaseAddress();

                    void ConfigureAuthHandler(IList<DelegatingHandler> handlers, IServiceProvider services)
                    {
                        var options = services
                            .GetRequiredService<IOptionsMonitor<TOptions>>()
                            .Get(clientName);

                        if (options is ICabazureAuthClientOptions authOptions)
                        {
                            var scope = authOptions.GetScope();
                            var credential = authOptions.GetCredential();

                            var tokenProvider = new BearerTokenProvider(
                                new TokenRequestContext([scope]),
                                credential,
                                new DateTimeProvider());

                            handlers.Add(new AzureAuthenticationHandler(tokenProvider));
                        }
                    }

                    return services.AddCabazureClient(
                        clientName,
                        jsonOptions,
                        builder => builder
                            .ConfigureHttpClient(ConfigureHttpClient)
                            .ConfigureAdditionalHttpMessageHandlers(ConfigureAuthHandler));
                }

                internal static partial IServiceCollection AddCabazureClient(
                    this IServiceCollection services,
                    string clientName,
                    Action<JsonSerializerOptions>? jsonOptions,
                    Action<IHttpClientBuilder> builder);
            }
            #nullable disable
            """));
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxContextReceiver is not ClientInitializationSyntaxReceiver receiver)
        {
            return;
        }

        foreach (var diagnostic in receiver.Diagnostics)
        {
            context.ReportDiagnostic(diagnostic);
        }

        var source = new StringBuilder();
        source.AppendLine("// <auto-generated/>");
        source.AppendLine("#nullable enable");

        var usingStatements = receiver.Endpoints
            .Select(e => e.Namespace)
            .OfType<string>()
            .Append("System.Text.Json")
            .Append("Cabazure.Client.Builder")
            .Append("Microsoft.Extensions.DependencyInjection.Extensions")
            .Distinct()
            .OrderByDescending(us => us.StartsWith("System"))
            .ThenBy(us => us)
            .ToArray();
        if (usingStatements.Length > 0)
        {
            foreach (var us in usingStatements)
            {
                source.AppendLine($"using {us};");
            }
            source.AppendLine();
        }

        source.AppendLine($$"""
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
            """);

        foreach (var endpoint in receiver.Endpoints)
        {
            source.AppendLine($$"""
                            services.TryAddSingleton<{{endpoint.InterfaceName}}, {{endpoint.ClassName}}>();
                    """);
        }

        source.AppendLine("""
                    return services;
                }
            """);

        source.AppendLine("}");
        source.AppendLine("#nullable disable");

        context.AddSource(
            $"ClientInitialization.Implementation.g.cs",
            source.ToString());
    }
}