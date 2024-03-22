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

    public static SyntaxToken? GetIdentifier(
        this SyntaxNode node)
        => node switch
        {
            BaseTypeDeclarationSyntax t => t.Identifier,
            PropertyDeclarationSyntax p => p.Identifier,
            MethodDeclarationSyntax m => m.Identifier,
            ParameterSyntax p => p.Identifier,
            _ => null,
        };
}
