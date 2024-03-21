using Cabazure.Client.SourceGenerator.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cabazure.Client.SourceGenerator.Descriptors;

public record InitializationMethodDescriptor(
    string Name,
    string Signature,
    string ServicesName,
    string? BuilderName,
    string? JsonOptionsName)
{
    public static InitializationMethodDescriptor? Create(
        Action<Diagnostic> diagnostics,
        SemanticModel semanticModel,
        AttributeSyntax attribute)
    {
        var method = attribute.FirstAncestorOrSelf<MethodDeclarationSyntax>();
        if (method == null)
        {
            return null;
        }

        string? servicesName = null;
        string? builderName = null;
        string? jsonOptionsName = null;
        foreach (var parameter in method.ParameterList.Parameters)
        {
            var parameterType = semanticModel.GetFullTypeName(parameter.Type!);
            if (parameterType == TypeConstants.ServiceCollection)
            {
                servicesName = parameter.Identifier.ValueText;
            }
            else if (parameterType == TypeConstants.BuilderDelegate)
            {
                builderName = parameter.Identifier.ValueText;
            }
            else if (parameterType == TypeConstants.JsonOptionsDelegate)
            {
                jsonOptionsName = parameter.Identifier.ValueText;
            }
            else
            {
                diagnostics.Invoke(
                    Diagnostic.Create(
                        DiagnosticDescriptors.UnsupportedInitializationParameter,
                        parameter.GetLocation(),
                        method.Identifier,
                        parameter.Identifier));
            }
        }

        if (servicesName == null)
        {
            diagnostics.Invoke(
                Diagnostic.Create(
                    DiagnosticDescriptors.MissingRequiredInitializationParameter,
                    method.GetLocation(),
                    method.Identifier,
                    TypeConstants.ServiceCollection));
            return null;
        }

        return new InitializationMethodDescriptor(
            Name: method.Identifier.ValueText,
            Signature: $"{method.Modifiers} {method.ReturnType} {method.Identifier}{method.ParameterList}",
            servicesName,
            builderName,
            jsonOptionsName);
    }
}
