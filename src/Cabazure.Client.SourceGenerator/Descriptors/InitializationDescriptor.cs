using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cabazure.Client.SourceGenerator.Descriptors;

public record InitializationDescriptor(
    TypeDescriptor DeclaringType,
    string ClientName,
    string MethodName,
    string Signature,
    string? ServicesName,
    string? BuilderName,
    string? JsonOptionsName)
{
    public static InitializationDescriptor FromAttribute(
        SemanticModel semanticModel,
        AttributeSyntax attribute)
    {
        var clientName = (string)semanticModel.GetConstantValue(attribute.ArgumentList!.Arguments[0].Expression).Value!;
        var method = (MethodDeclarationSyntax)attribute.Parent!.Parent!;
        var methodName = method.Identifier.ValueText;
        var methodSignature = $"{method.Modifiers} {method.ReturnType} {method.Identifier}{method.ParameterList}";

        var type = TypeDescriptor.FromType((TypeDeclarationSyntax)method.Parent!);

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
        }

        return new(
            type,
            clientName,
            methodName,
            methodSignature,
            servicesName,
            builderName,
            jsonOptionsName);
    }
}
