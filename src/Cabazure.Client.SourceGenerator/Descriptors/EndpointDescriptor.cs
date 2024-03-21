using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cabazure.Client.SourceGenerator.Descriptors;

public record EndpointDescriptor(
    string ClientName,
    string InterfaceName,
    string ClassName,
    string? Namespace)
{
    public static EndpointDescriptor Create(
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

        return new(
            clientName,
            interfaceName,
            className,
            ns);
    }

    public static EndpointDescriptor Create(
        InterfaceDeclarationSyntax interfaceSyntax,
        GeneratorAttributeSyntaxContext context)
    {
        var clientName = (string)context.Attributes[0].ConstructorArguments[0].Value!;
        var interfaceName = interfaceSyntax.Identifier.ValueText;
        var className = interfaceName.Length > 1 && interfaceName[0] == 'I'
           ? interfaceName.Substring(1)
           : interfaceName + "_Implementation";
        var ns = interfaceSyntax.GetNamespace();

        return new(
            clientName,
            interfaceName,
            className,
            ns);
    }
}
