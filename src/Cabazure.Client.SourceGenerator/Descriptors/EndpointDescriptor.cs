using System.Collections.Immutable;
using Cabazure.Client.SourceGenerator.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cabazure.Client.SourceGenerator.Descriptors;

public record EndpointDescriptor(
    string ClientName,
    string InterfaceName,
    string ClassName,
    string? Namespace,
    ImmutableArray<string> Usings,
    ImmutableArray<EndpointMethodDescriptor> Methods)
{
    public static EndpointDescriptor? Create(
        Action<Diagnostic> diagnostics,
        GeneratorAttributeSyntaxContext context)
    {
        if (context.TargetNode is not InterfaceDeclarationSyntax @interface)
        {
            diagnostics.Invoke(
                Diagnostic.Create(
                    DiagnosticDescriptors.UnsupportedEndpointReturnType,
                    context.TargetNode.GetLocation(),
                    context.TargetNode.GetIdentifier()));

            return null;
        }

        var clientName = context.Attributes
            .SelectMany(a => a.ConstructorArguments)
            .Select(a => a.Value)
            .OfType<string>()
            .FirstOrDefault();
        if (clientName == null)
        {
            return null;
        }

        var interfaceName = @interface.Identifier.ValueText;
        var className = interfaceName.Length > 1 && interfaceName[0] == 'I'
           ? interfaceName.Substring(1)
           : interfaceName + "_Implementation";
        var ns = @interface.GetNamespace();

        var usings = @interface
            .GetUsings()
            .Select(u => u.Name?.ToString())
            .OfType<string>()
            .ToImmutableArray();

        var methods = @interface
            .Members
            .OfType<MethodDeclarationSyntax>()
            .Select(m => EndpointMethodDescriptor.Create(
                diagnostics,
                context.SemanticModel,
                m))
            .OfType<EndpointMethodDescriptor>()
            .ToImmutableArray();

        return new EndpointDescriptor(
            clientName,
            interfaceName,
            className,
            ns,
            usings,
            methods);
    }
}
