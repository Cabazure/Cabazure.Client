using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cabazure.Client.SourceGenerator.Descriptors;

public record EndpointReferenceDescriptor(
    string ClientName,
    string InterfaceName,
    string ClassName,
    string? Namespace)
{
    public static EndpointReferenceDescriptor? Create(
        Action<Diagnostic> diagnostics,
        SemanticModel semanticModel,
        AttributeSyntax attribute)
    {
        var clientName = (string)semanticModel.GetConstantValue(attribute.ArgumentList!.Arguments[0].Expression).Value!;
        var interfaceSyntax = (InterfaceDeclarationSyntax)attribute.Parent!.Parent!;
        var interfaceName = interfaceSyntax.Identifier.ValueText;
        var className = interfaceName.Length > 1 && interfaceName[0] == 'I'
            ? interfaceName.Substring(1)
            : interfaceName + "_Implementation";
        var ns = interfaceSyntax.GetNamespace();

        return new EndpointReferenceDescriptor(
            clientName,
            interfaceName,
            className,
            ns);
    }
}
