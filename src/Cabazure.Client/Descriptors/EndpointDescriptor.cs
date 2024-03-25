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
            return null;
        }

        var clientName = context.Attributes
            .SelectMany(a => a.ConstructorArguments)
            .Select(a => a.Value)
            .OfType<string>()
            .FirstOrDefault();
        if (string.IsNullOrWhiteSpace(clientName))
        {
            diagnostics.Invoke(
                Diagnostic.Create(
                    DiagnosticDescriptors.ClientNameIsEmpty,
                    @interface.GetLocation(),
                    @interface.Identifier));

            return null;
        }

        var interfaceName = @interface.Identifier.ValueText;
        var className = interfaceName.Length > 1 && interfaceName[0] == 'I'
           ? interfaceName.Substring(1)
           : interfaceName + "_Implementation";

        var interfaceParent = @interface.Parent;
        while (interfaceParent is ClassDeclarationSyntax c)
        {
            interfaceName = string.Concat(c.Identifier.ValueText, ".", interfaceName);
            interfaceParent = c.Parent;
        }

        var ns = @interface.GetNamespace();

        var usings = @interface
            .GetUsings()
            .Select(u => u.Name?.ToString())
            .OfType<string>()
            .ToImmutableArray();

        var methodDeclarations = @interface.Members.OfType<MethodDeclarationSyntax>().ToArray();
        if (methodDeclarations.Length == 0)
        {
            diagnostics.Invoke(
                Diagnostic.Create(
                    DiagnosticDescriptors.EndpointHasNoMethods,
                    @interface.GetLocation(),
                    @interface.Identifier));

            return null;
        }

        var methods = methodDeclarations
            .Select(m => EndpointMethodDescriptor.Create(
                diagnostics,
                context.SemanticModel,
                m))
            .OfType<EndpointMethodDescriptor>()
            .ToImmutableArray();

        if (methods.Length == 0)
        {
            return null;
        }

        return new EndpointDescriptor(
            clientName,
            interfaceName,
            className,
            ns,
            usings,
            methods);
    }
}
