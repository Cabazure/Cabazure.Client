using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cabazure.Client.SourceGenerator;

public static class RoslynExtensions
{
    public static string? GetNamespace(
        this SyntaxNode node)
        => node.SyntaxTree
            .GetCompilationUnitRoot()
            .ChildNodes()
            .OfType<BaseNamespaceDeclarationSyntax>()
            .Select(ns => ns.Name.ToString())
            .FirstOrDefault();

    public static SyntaxList<UsingDirectiveSyntax> GetUsings(
        this SyntaxNode node)
        => node.SyntaxTree
            .GetCompilationUnitRoot()
            .Usings;

    public static string? GetName(
        this SyntaxNode node)
        => node switch
        {
            BaseTypeDeclarationSyntax t => t.Identifier.ValueText,
            _ => null,
        };

    public static bool IsPartial(
        this TypeDeclarationSyntax type)
        => type.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));

    public static bool IsPartial(
        this MethodDeclarationSyntax type)
        => type.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));
}
