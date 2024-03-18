using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cabazure.Client.SourceGenerator;

[Generator]
public class ClientInitializationGenerator : ISourceGenerator, ISyntaxContextReceiver
{
    private readonly Dictionary<string, string> endpoints = [];
    private readonly HashSet<string> namespaces = [];

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => this);
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var source = new StringBuilder();
        if (namespaces.Count > 0)
        {
            foreach (var ns in namespaces)
            {
                source.AppendLine($"using {ns};");
            }
            source.AppendLine();
        }

        source.AppendLine("""
            namespace Microsoft.Extensions.DependencyInjection;

            public static partial class ServiceCollectionExtensions
            {
            """);

        var clients = endpoints.GroupBy(e => e.Value, e => e.Key);
        foreach (var client in clients)
        {
            source.AppendLine($$"""
                    public static IServiceCollection Add{{client.Key}}Client(
                        this IServiceCollection services)
                    {
                """);

            foreach (var endpoint in client.AsEnumerable())
            {
                var className = endpoint.Length > 1 && endpoint[0] == 'I'
                    ? endpoint.Substring(1)
                    : endpoint + "_Implementation";

                source.AppendLine($$"""
                            services.AddSingleton<{{endpoint}}, {{className}}>();
                    """);
            }

            source.AppendLine("""

                        return services;
                    }
                """);
        }

        source.AppendLine("}");

        context.AddSource(
            "ServiceCollectionExtensions.g.cs",
            source.ToString());
    }

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        if (context.Node is not InterfaceDeclarationSyntax i)
        {
            return;
        }

        var clientName = i.AttributeLists
            .SelectMany(al => al.Attributes)
            .Where(a => context.SemanticModel.GetSymbolInfo(a).Symbol?.ContainingType.ToString() == TypeConstants.ClientEndpointAttribute)
            .Select(a => context.SemanticModel.GetConstantValue(a.ArgumentList!.Arguments[0].Expression).Value)
            .OfType<string>()
            .FirstOrDefault();

        if (clientName != null)
        {
            endpoints.Add(i.Identifier.ValueText, clientName);

            var ns = i.SyntaxTree
                .GetCompilationUnitRoot()
                .ChildNodes()
                .OfType<BaseNamespaceDeclarationSyntax>()
                .Select(ns => ns.Name.ToString())
                .FirstOrDefault();
            if (ns != null)
            {
                namespaces.Add(ns);
            }
        }
    }
}
