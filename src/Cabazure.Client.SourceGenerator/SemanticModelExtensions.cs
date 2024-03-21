using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cabazure.Client.SourceGenerator;

public static class SemanticModelExtensions
{
    public static string? GetFullTypeName(
        this SemanticModel semanticModel,
        TypeSyntax type)
        => semanticModel
            .GetTypeInfo(type)
            .Type?
            .ToDisplayString(
                SymbolDisplayFormat.FullyQualifiedFormat
                .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.OmittedAsContaining));
}
