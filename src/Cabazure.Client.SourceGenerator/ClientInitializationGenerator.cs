﻿using System.Text;
using Microsoft.CodeAnalysis;

namespace Cabazure.Client.SourceGenerator;

[Generator]
public class ClientInitializationGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new ClientInitializationSyntaxReceiver());
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

        foreach (var classGroup in receiver.Initializations.GroupBy(i => i.Class))
        {
            var type = classGroup.Key;
            var source = new StringBuilder();
            source.AppendLine("// <auto-generated/>");

            var ns = type.Namespace;
            var usingStatements = receiver.Endpoints
                .Select(e => e.Namespace)
                .OfType<string>()
                .Append("System.Text.Json")
                .Append("Cabazure.Client.Builder")
                .Append("Microsoft.Extensions.DependencyInjection")
                .Append("Microsoft.Extensions.DependencyInjection.Extensions")
                .Distinct()
                .Where(us => us != ns)
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

            if (ns != null)
            {
                source.AppendLine($"namespace {ns};\n");
            }

            source.AppendLine($$"""
                {{type.Signature}}
                {
                """);

            foreach (var initialization in classGroup)
            {
                source.AppendLine($$"""
                        {{initialization.Method.Signature}}
                        {
                            var clientBuilder = services.AddHttpClient("{{initialization.ClientName}}");
                            {{initialization.Method.BuilderName}}?.Invoke(clientBuilder);

                            {{initialization.Method.ServicesName}}
                                .AddOptions<JsonSerializerOptions>("{{initialization.ClientName}}")
                                .Configure({{initialization.Method.JsonOptionsName}});

                            {{initialization.Method.ServicesName}}.TryAddSingleton<IClientSerializer, ClientSerializer>();
                            {{initialization.Method.ServicesName}}.TryAddSingleton<IMessageRequestFactory, MessageRequestFactory>();

                    """);

                var clientEndpoints = receiver.Endpoints
                    .Where(e => e.ClientName == initialization.ClientName);

                foreach (var endpoint in clientEndpoints)
                {
                    source.AppendLine($$"""
                                {{initialization.Method.ServicesName}}.AddSingleton<{{endpoint.InterfaceName}}, {{endpoint.ClassName}}>();
                        """);
                }

                source.AppendLine("""
                        }
                    """);
            }

            source.AppendLine("}");

            context.AddSource(
                $"{type.Name}.g.cs",
                source.ToString());
        }
    }
}