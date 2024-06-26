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
            using System.Text.Json;
            using Cabazure.Client;

            namespace Microsoft.Extensions.DependencyInjection;

            internal static partial class ClientInitialization
            {
                internal static partial IServiceCollection AddCabazureClient(
                    this IServiceCollection services,
                    string clientName,
                    Action<IHttpClientBuilder> builder,
                    Action<JsonSerializerOptions> jsonOptions);
            }
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
                    Action<IHttpClientBuilder> builder,
                    Action<JsonSerializerOptions> jsonOptions)
                {
                    var clientBuilder = services.AddHttpClient(clientName);
                    builder.Invoke(clientBuilder);
            
                    services
                        .AddOptions<JsonSerializerOptions>(clientName)
                        .Configure(jsonOptions);
            
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

        context.AddSource(
            $"ClientInitialization.Implementation.g.cs",
            source.ToString());
    }
}