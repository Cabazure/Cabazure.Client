using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cabazure.Client.SourceGenerator;

public static class SemanticModelExtensions
{
    public static string? GetTypeName(
        this SemanticModel semanticModel,
        TypeSyntax type)
        => semanticModel
            .GetTypeInfo(type)
            .Type?
            .GetName();

    public static string? GetTypeName(
        this SemanticModel semanticModel,
        AttributeSyntax attribute)
        => semanticModel
            .GetSymbolInfo(attribute)
            .Symbol?
            .ContainingType?
            .GetName();

    public static string? GetName(
        this ITypeSymbol typeSymbol)
        => typeSymbol
            .ToDisplayString(
                SymbolDisplayFormat.FullyQualifiedFormat
                    .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.OmittedAsContaining)
                    .WithGenericsOptions(SymbolDisplayGenericsOptions.None));

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
