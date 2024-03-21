using System.Collections.Immutable;
using Cabazure.Client.SourceGenerator.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cabazure.Client.SourceGenerator.Descriptors;

public record EndpointMethodDescriptor(
    string Name,
    string Signature,
    string? ResultType,
    string HttpMethod,
    string RouteTemplate,
    string? OptionsParameter,
    string? CancellationTokenParameter,
    ImmutableArray<EndpointParameter> HeaderParameters,
    ImmutableArray<EndpointParameter> QueryParameters,
    ImmutableArray<EndpointParameter> PathParameters,
    string? BodyParameter)
{
    public static EndpointMethodDescriptor? Create(
        Action<Diagnostic> diagnostics,
        SemanticModel semanticModel,
        MethodDeclarationSyntax method)
    {
        string? httpMethod = null;
        string? routeTemplate = null;
        foreach (var attribute in method.AttributeLists.SelectMany(a => a.Attributes))
        {
            var attributeTypeName = semanticModel.GetFullTypeName(attribute);
            switch (attributeTypeName)
            {
                case TypeConstants.GetAttribute:
                    httpMethod = nameof(System.Net.Http.HttpMethod.Get);
                    routeTemplate = semanticModel.GetAttributeValue(attribute);
                    break;

                case TypeConstants.PostAttribute:
                    httpMethod = nameof(System.Net.Http.HttpMethod.Post);
                    routeTemplate = semanticModel.GetAttributeValue(attribute);
                    break;

                case TypeConstants.PutAttribute:
                    httpMethod = nameof(System.Net.Http.HttpMethod.Put);
                    routeTemplate = semanticModel.GetAttributeValue(attribute);
                    break;

                case TypeConstants.DeleteAttribute:
                    httpMethod = nameof(System.Net.Http.HttpMethod.Delete);
                    routeTemplate = semanticModel.GetAttributeValue(attribute);
                    break;
            }
        }

        if (httpMethod == null || routeTemplate == null)
        {
            diagnostics.Invoke(Diagnostic.Create(
                DiagnosticDescriptors.MissingEndpointRoute,
                method.GetLocation(),
                method.Parent?.GetIdentifier(),
                method.Identifier));

            return null;
        }

        if (!IsValidEndpointReturnType(method, out var returnType))
        {
            diagnostics.Invoke(
                Diagnostic.Create(
                    DiagnosticDescriptors.UnsupportedEndpointReturnType,
                    method.GetLocation(),
                    method.Identifier));

            return null;
        }

        string? optionsParameter = null;
        var headerParameters = new List<EndpointParameter>();
        var queryParameters = new List<EndpointParameter>();
        var pathParameters = new List<EndpointParameter>();
        string? bodyParameter = null;
        string? cancellationTokenParameter = null;

        foreach (var parameter in method.ParameterList.Parameters)
        {
            var typeName = semanticModel.GetFullTypeName(parameter.Type!);
            var parameterName = parameter.Identifier.ValueText;

            if (typeName == TypeConstants.ClientRequestOptions)
            {
                optionsParameter = parameterName;
                continue;
            }

            if (typeName == TypeConstants.CancellationToken)
            {
                cancellationTokenParameter = parameterName;
                continue;
            }

            foreach (var attribute in parameter.AttributeLists.SelectMany(a => a.Attributes))
            {
                var attributeTypeName = semanticModel.GetFullTypeName(attribute);
                if (attributeTypeName == TypeConstants.BodyAttribute)
                {
                    bodyParameter = parameterName;
                    continue;
                }

                var attributeValue = semanticModel.GetAttributeValue(attribute);
                if (attributeTypeName == TypeConstants.PathAttribute)
                {
                    pathParameters.Add(new(parameterName, attributeValue));
                    continue;
                }

                if (attributeTypeName == TypeConstants.QueryAttribute)
                {
                    queryParameters.Add(new(parameterName, attributeValue));
                    continue;
                }

                if (attributeTypeName == TypeConstants.HeaderAttribute)
                {
                    headerParameters.Add(new(parameterName, attributeValue));
                    continue;
                }
            }
        }

        return new EndpointMethodDescriptor(
            method.Identifier.ValueText,
            $"public async {method.ReturnType} {method.Identifier}{method.ParameterList}",
            returnType?.ToString(),
            httpMethod,
            routeTemplate,
            optionsParameter,
            cancellationTokenParameter,
            headerParameters.ToImmutableArray(),
            queryParameters.ToImmutableArray(),
            pathParameters.ToImmutableArray(),
            bodyParameter);
    }

    private static bool IsValidEndpointReturnType(MethodDeclarationSyntax method, out TypeSyntax? resultType)
    {
        resultType = null;
        if (method.ReturnType is not GenericNameSyntax { } taskType)
        {
            return false;
        }

        if (taskType.Identifier.ToString() == "System.Threading.Task")
        {
            return false;
        }

        if (taskType.TypeArgumentList.Arguments[0] is not SimpleNameSyntax endpointType)
        {
            return false;
        }

        if (endpointType.Identifier.ToString() == "Cabazure.Client.EndpointResponse")
        {
            return false;
        }

        if (endpointType is GenericNameSyntax g)
        {
            resultType = g.TypeArgumentList.Arguments[0];
        }

        return true;
    }
}
