using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cabazure.Client.SourceGenerator.Descriptors;

public record TypeDescriptor(
    string Name,
    string? Namespace,
    string Signature,
    bool IsPartialClass)
{
    public static TypeDescriptor FromType(
        TypeDeclarationSyntax type)
    {
        var typeName = type.Identifier.ValueText;
        var typeNamespace = type.GetNamespace();
        var typeSignature = $"{type.Modifiers} {type.Keyword} {type.Identifier}";
        var isPartialClass = type is ClassDeclarationSyntax c && c.IsPartial();

        return new(
            typeName,
            typeNamespace,
            typeSignature,
            isPartialClass);
    }
}
