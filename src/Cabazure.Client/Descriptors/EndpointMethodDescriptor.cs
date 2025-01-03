﻿using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Cabazure.Client.SourceGenerator.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cabazure.Client.SourceGenerator.Descriptors;

public record EndpointMethodDescriptor(
    string Name,
    ImmutableArray<string> Parameters,
    string ReturnType,
    string? ResponseType,
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
    private static readonly Regex TokenRegex = new(
        "\\{(?<id>[^}]*?)\\}",
        RegexOptions.Compiled,
        TimeSpan.FromMilliseconds(100));

    public static EndpointMethodDescriptor? Create(
        Action<Diagnostic> diagnostics,
        SemanticModel semanticModel,
        MethodDeclarationSyntax method)
    {
        TryGetEndpointRoute(semanticModel, method, out var httpMethod, out var routeTemplate);

        if (httpMethod == null || routeTemplate == null)
        {
            diagnostics.Invoke(Diagnostic.Create(
                DiagnosticDescriptors.MissingEndpointRoute,
                method.GetLocation(),
                method.Parent?.GetIdentifier(),
                method.Identifier));

            return null;
        }

        if (!IsValidEndpointReturnType(semanticModel, method, out var returnType, out var responseType))
        {
            diagnostics.Invoke(
                Diagnostic.Create(
                    DiagnosticDescriptors.UnsupportedEndpointReturnType,
                    method.GetLocation(),
                    method.Parent?.GetIdentifier(),
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
            var parameterType = semanticModel.GetTypeInfo(parameter.Type!).Type!;
            var parameterTypeName = parameterType.GetName()!;

            var isNullable = parameter.Type!.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.NullableType);
            var parameterName = parameter.Identifier.ValueText;

            if (parameterType.AllInterfaces.Any(i => i.GetName() == TypeConstants.RequestOptions))
            {
                optionsParameter = parameterName;
                continue;
            }

            if (parameterTypeName == TypeConstants.CancellationToken)
            {
                cancellationTokenParameter = parameterName;
                continue;
            }

            var isValid = false;
            foreach (var attribute in parameter.AttributeLists.SelectMany(a => a.Attributes))
            {
                var attributeTypeName = semanticModel.GetTypeName(attribute);
                if (attributeTypeName == TypeConstants.BodyAttribute)
                {
                    isValid = true;
                    bodyParameter = parameterName;
                    continue;
                }

                var attributeValues = semanticModel.GetAttributeValues(attribute);
                var name = attributeValues.GetValueOrDefault("name") ?? parameterName;
                var formatString = attributeValues.GetValueOrDefault("formatString");

                if (attributeTypeName == TypeConstants.PathAttribute)
                {
                    if (!routeTemplate.Contains($"{{{name}}}"))
                    {
                        diagnostics.Invoke(
                            Diagnostic.Create(
                                DiagnosticDescriptors.PathParameterNotInRouteTemplate,
                                parameter.GetLocation(),
                                method.Parent?.GetIdentifier(),
                                method.Identifier,
                                parameter.Identifier,
                                routeTemplate,
                                name));
                    }
                    isValid = true;
                    pathParameters.Add(new(name, parameterName, parameterTypeName, isNullable, formatString));
                    continue;
                }

                if (attributeTypeName == TypeConstants.QueryAttribute)
                {
                    isValid = true;
                    queryParameters.Add(new(name, parameterName, parameterTypeName, isNullable, formatString));
                    continue;
                }

                if (attributeTypeName == TypeConstants.HeaderAttribute)
                {
                    isValid = true;
                    headerParameters.Add(new(name, parameterName, parameterTypeName, isNullable, formatString));
                    continue;
                }
            }

            if (!isValid)
            {
                diagnostics.Invoke(
                    Diagnostic.Create(
                        DiagnosticDescriptors.UnsupportedEndpointParameter,
                        parameter.GetLocation(),
                        method.Parent?.GetIdentifier(),
                        method.Identifier,
                        parameter.Identifier));

                return null;
            }
        }

        foreach (Match m in TokenRegex.Matches(routeTemplate))
        {
            var token = m.Groups["id"].Value;
            if (!pathParameters.Any(p => p.Name == token))
            {
                diagnostics.Invoke(
                    Diagnostic.Create(
                        DiagnosticDescriptors.RouteTemplateHasUnmappedPathParameter,
                        method.GetLocation(),
                        method.Parent?.GetIdentifier(),
                        method.Identifier,
                        token,
                        routeTemplate));
            }
        }

        return new EndpointMethodDescriptor(
            method.Identifier.ValueText,
            method.ParameterList.Parameters.Select(p => $"{p.Type} {p.Identifier}").ToImmutableArray(),
            method.ReturnType.ToString(),
            responseType,
            returnType,
            httpMethod,
            routeTemplate,
            optionsParameter,
            cancellationTokenParameter,
            headerParameters.ToImmutableArray(),
            queryParameters.ToImmutableArray(),
            pathParameters.ToImmutableArray(),
            bodyParameter);
    }

    private static bool TryGetEndpointRoute(
        SemanticModel semanticModel,
        MethodDeclarationSyntax method,
        [NotNullWhen(true)] out string? httpMethod,
        [NotNullWhen(true)] out string? routeTemplate)
    {
        httpMethod = null;
        routeTemplate = null;
        foreach (var attribute in method.AttributeLists.SelectMany(a => a.Attributes))
        {
            var attributeTypeName = semanticModel.GetTypeName(attribute);
            switch (attributeTypeName)
            {
                case TypeConstants.GetAttribute:
                    httpMethod = nameof(System.Net.Http.HttpMethod.Get);
                    routeTemplate = semanticModel.GetAttributeValue(attribute)!;
                    return true;

                case TypeConstants.PostAttribute:
                    httpMethod = nameof(System.Net.Http.HttpMethod.Post);
                    routeTemplate = semanticModel.GetAttributeValue(attribute)!;
                    return true;

                case TypeConstants.PutAttribute:
                    httpMethod = nameof(System.Net.Http.HttpMethod.Put);
                    routeTemplate = semanticModel.GetAttributeValue(attribute)!;
                    return true;

                case TypeConstants.DeleteAttribute:
                    httpMethod = nameof(System.Net.Http.HttpMethod.Delete);
                    routeTemplate = semanticModel.GetAttributeValue(attribute)!;
                    return true;

            }
        }

        return false;
    }

    private static bool IsValidEndpointReturnType(
        SemanticModel semanticModel,
        MethodDeclarationSyntax method,
        out string? resultType,
        out string? responseType)
    {
        resultType = null;
        responseType = null;

        if (method.ReturnType is not GenericNameSyntax { } taskType)
        {
            return false;
        }

        if (semanticModel.GetTypeName(taskType) != TypeConstants.Task)
        {
            return false;
        }

        if (taskType.TypeArgumentList.Arguments[0] is not TypeSyntax endpointType)
        {
            return false;
        }

        if (semanticModel.GetTypeName(endpointType)
            is not TypeConstants.EndpointResponse
            and not TypeConstants.PagedResponse)
        {
            return false;
        }

        if (endpointType is GenericNameSyntax g && g.TypeArgumentList.Arguments[0] is TypeSyntax r)
        {
            responseType = endpointType.ToString();
            resultType = r.ToString();
        }

        return true;
    }
}
