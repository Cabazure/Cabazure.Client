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

    public static string? GetFullTypeName(
        this SemanticModel semanticModel,
        AttributeSyntax attribute)
        => semanticModel
            .GetSymbolInfo(attribute)
            .Symbol?
            .ContainingType?
            .ToDisplayString(
                SymbolDisplayFormat.FullyQualifiedFormat
                .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.OmittedAsContaining));

    public static string? GetAttributeValue(
        this SemanticModel semanticModel,
        AttributeSyntax attribute)
    {
        var expression = attribute
            .ArgumentList?
            .Arguments
            .Select(a => a.Expression)
            .FirstOrDefault();

        return expression != null
            ? semanticModel.GetConstantValue(expression).Value as string
            : null;
    }
}
