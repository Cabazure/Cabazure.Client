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
        var interfaceName = interfaceSyntax.Identifier.ValueText;
        var className = interfaceName.Length > 1 && interfaceName[0] == 'I'
            ? interfaceName.Substring(1)
            : interfaceName + "_Implementation";

        var interfaceParent = interfaceSyntax.Parent;
        while (interfaceParent is ClassDeclarationSyntax c)
        {
            interfaceName = string.Concat(c.Identifier.ValueText, ".", interfaceName);
            interfaceParent = c.Parent;
        }

        var ns = interfaceSyntax.GetNamespace();

        return new EndpointReferenceDescriptor(
            clientName,
            interfaceName,
            className,
            ns);
    }
}
