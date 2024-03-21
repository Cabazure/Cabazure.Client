using Cabazure.Client.SourceGenerator.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cabazure.Client.SourceGenerator.Descriptors;

public record InitializationClassDescriptor(
    string Name,
    string? Namespace,
    string Signature)
{
    public static InitializationClassDescriptor? Create(
        Action<Diagnostic> diagnostics,
        AttributeSyntax attribute)
    {
        var classNode = attribute.FirstAncestorOrSelf<ClassDeclarationSyntax>();
        if (classNode == null || !classNode.IsPartial())
        {
            var node = (SyntaxNode?)classNode ?? attribute;
            diagnostics.Invoke(
                Diagnostic.Create(
                    DiagnosticDescriptors.InitializationNotInPartialClass,
                    node.GetLocation(),
                    node.GetIdentifier()));

            return null;
        }

        return new InitializationClassDescriptor(
            Name: classNode.Identifier.ValueText,
            Namespace: classNode.GetNamespace(),
            Signature: $"{classNode.Modifiers} {classNode.Keyword} {classNode.Identifier}");
    }
}
