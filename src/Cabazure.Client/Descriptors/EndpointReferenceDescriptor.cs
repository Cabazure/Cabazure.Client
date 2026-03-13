using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cabazure.Client.SourceGenerator.Descriptors;

public record EndpointReferenceDescriptor(
    string ClientName,
    string InterfaceName,
    string ClassName,
    string? Namespace)
{
    public static EndpointReferenceDescriptor Create(
        GeneratorAttributeSyntaxContext context)
    {
        var clientName = context.Attributes
            .SelectMany(a => a.ConstructorArguments)
            .Select(a => a.Value)
            .OfType<string>()
            .FirstOrDefault();

        var interfaceSyntax = (InterfaceDeclarationSyntax)context.TargetNode;
        var (interfaceName, className) = interfaceSyntax.GetEndpointNames();

        var ns = interfaceSyntax.GetNamespace();

        return new EndpointReferenceDescriptor(
            clientName,
            interfaceName,
            className,
            ns);
    }
}
