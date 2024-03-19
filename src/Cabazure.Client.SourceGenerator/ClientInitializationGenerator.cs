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
    private readonly Dictionary<MethodDeclarationSyntax, string> initMethods = [];

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => this);
    }

    public void Execute(GeneratorExecutionContext context)
    {
        try
        {
            GenerateInitialization(context);
        }
        catch (InvalidOperationException ex)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "CLIENT0001",
                        "Cabazure.Client",
                        "Failed to generate code: " + ex.Message,
                        nameof(ClientInitializationGenerator),
                        DiagnosticSeverity.Error,
                        true),
                    Location.None));
        }
    }

    private void GenerateInitialization(GeneratorExecutionContext context)
    {
        foreach (var initMethods in initMethods.GroupBy(p => p.Key.Parent))
        {
            if (initMethods.Key is not ClassDeclarationSyntax type || !type.IsPartial())
            {
                throw new InvalidOperationException(
                    $"{TypeConstants.ClientInitializationAttribute} must be defined on a method of a partial class.");
            }

            var source = new StringBuilder();
            var ns = type.GetNamespace();
            var usingStatements = namespaces
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
            {{type.Modifiers}} class {{type.Identifier}}
            {
            """);

            foreach (var initMethod in initMethods)
            {
                var method = initMethod.Key;
                var clientName = initMethod.Value;

                source.AppendLine($$"""
                        {{method.Modifiers}} {{method.ReturnType}} {{method.Identifier}}{{method.ParameterList}}
                        {
                            var clientBuilder = services.AddHttpClient("{{clientName}}");
                            builder?.Invoke(clientBuilder);

                            services
                                .AddOptions<JsonSerializerOptions>("{{clientName}}")
                                .Configure(jsonOptions ?? new Action<JsonSerializerOptions>(_ => { }));

                            services.TryAddSingleton<IClientSerializer, ClientSerializer>();
                            services.TryAddSingleton<IMessageRequestFactory, MessageRequestFactory>();

                    """);

                var includedEndpoints = endpoints
                    .Where(e => e.Value == clientName)
                    .Select(e => e.Key);
                foreach (var endpoint in includedEndpoints)
                {
                    var className = endpoint.Length > 1 && endpoint[0] == 'I'
                        ? endpoint.Substring(1)
                        : endpoint + "_Implementation";

                    source.AppendLine($$"""
                                services.AddSingleton<{{endpoint}}, {{className}}>();
                        """);
                }

                source.AppendLine("""
                        }
                    """);
            }

            source.AppendLine("}");

            context.AddSource(
                $"{type.Identifier.ValueText}.g.cs",
                source.ToString());
        }
    }

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        if (context.Node is not AttributeSyntax attribute)
        {
            return;
        }

        var attributeType = context.SemanticModel
            .GetSymbolInfo(attribute).Symbol?
            .ContainingType
            .ToString();

        if (attributeType == TypeConstants.ClientEndpointAttribute)
        {
            var value = context.SemanticModel
                .GetConstantValue(attribute.ArgumentList!.Arguments[0].Expression)
                .Value;

            var parent = attribute.Parent?.Parent;

            if (value is string clientName && parent is InterfaceDeclarationSyntax i)
            {
                endpoints.Add(i.Identifier.ValueText, clientName);

                if (attribute.GetNamespace() is { } ns)
                {
                    namespaces.Add(ns);
                }
            }
        }
        else if (attributeType == TypeConstants.ClientInitializationAttribute)
        {
            var value = context.SemanticModel
                .GetConstantValue(attribute.ArgumentList!.Arguments[0].Expression)
                .Value;

            var parent = attribute.Parent?.Parent;

            if (value is string clientName && parent is MethodDeclarationSyntax m)
            {
                initMethods.Add(m, clientName);
            }
        }
    }
}
