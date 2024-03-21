﻿using System.Text;
using Cabazure.Client.SourceGenerator.Descriptors;
using Cabazure.Client.SourceGenerator.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cabazure.Client.SourceGenerator;

[Generator]
public class ClientEndpointGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var endpointsToGenerate = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                TypeConstants.ClientEndpointAttribute,
                static (_, _) => true,
                static (ctx, _) => ctx);

        context.RegisterSourceOutput(
            endpointsToGenerate,
            (c, s) => ProcessEndpoint(c, s));
    }

    private static void ProcessEndpoint(
        SourceProductionContext context,
        GeneratorAttributeSyntaxContext syntax)
    {
        if (syntax.TargetNode is not InterfaceDeclarationSyntax endpointSyntax)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticDescriptors.UnsupportedEndpointReturnType,
                    syntax.TargetNode.GetLocation(),
                    syntax.TargetNode.GetIdentifier()));

            return;
        }

        var endpoint = EndpointDescriptor.Create(
            endpointSyntax,
            syntax);

        var source = new StringBuilder();
        source.AppendLine("// <auto-generated/>");

        var ns = endpointSyntax.GetNamespace();
        var usingStatements = endpointSyntax
            .GetUsings()
            .Select(u => u.Name?.ToString())
            .OfType<string>()
            .Append("System.Net")
            .Append("Cabazure.Client")
            .Append("Cabazure.Client.Builder")
            .Distinct()
            .Where(us => us != ns)
            .OrderByDescending(us => us.StartsWith("System"))
            .ThenBy(us => us)
            .ToArray();
        if (usingStatements.Length > 0)
        {
            foreach (var us in usingStatements)
            {
                source.AppendLine($"using {us};");
            }
            source.AppendLine();
        }

        if (ns != null)
        {
            source.AppendLine($"namespace {ns};\n");
        }

        source.AppendLine($$"""
            public partial class {{endpoint.ClassName}} : {{endpoint.InterfaceName}}
            {
                private readonly IHttpClientFactory factory;
                private readonly IMessageRequestFactory requestFactory;

                public {{endpoint.ClassName}}(
                    IHttpClientFactory factory,
                    IMessageRequestFactory requestFactory)
                {
                    this.factory = factory;
                    this.requestFactory = requestFactory;
                }
            """);

        var methods = endpointSyntax.Members
            .OfType<MethodDeclarationSyntax>();

        foreach (var methodSyntax in methods)
        {
            GenerateEndpointMethod(
                context,
                source,
                methodSyntax,
                endpoint.ClientName,
                syntax.SemanticModel);
        }

        source.AppendLine("}");

        context.AddSource(
            $"{endpoint.ClassName}.g.cs",
            source.ToString());
    }

    private static void GenerateEndpointMethod(
        SourceProductionContext context,
        StringBuilder source,
        MethodDeclarationSyntax method,
        string clientName,
        SemanticModel semanticModel)
    {
        if (!IsValidEndpointReturnType(method, out var returnType))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.UnsupportedEndpointReturnType,
                method.GetLocation(),
                method.Identifier));

            return;
        }

        string? returnTypeString = null;
        if (returnType is { } g)
        {
            returnTypeString = $"<{g}>";
        }

        string? httpMethod = null;
        string? routeTemplate = null;
        foreach (var attList in method.AttributeLists)
        {
            foreach (var att in attList.Attributes)
            {
                var attribute = semanticModel.GetSymbolInfo(att).Symbol?.ContainingType?.ToString();
                if (attribute == TypeConstants.GetAttribute)
                {
                    httpMethod = nameof(HttpMethod.Get);
                    routeTemplate = att.ArgumentList!.Arguments[0].ToFullString();
                    continue;
                }

                if (attribute == TypeConstants.PostAttribute)
                {
                    httpMethod = nameof(HttpMethod.Post);
                    routeTemplate = att.ArgumentList!.Arguments[0].ToFullString();
                    continue;
                }
            }
        }

        if (httpMethod == null || routeTemplate == null)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.MissingEndpointRoute,
                method.GetLocation(),
                method.Parent?.GetIdentifier(),
                method.Identifier));

            return;
        }

        var clientOptions = new StringBuilder();
        var requestOptions = new StringBuilder();
        var cancellationToken = "CancellationToken.None";
        foreach (var parameter in method.ParameterList.Parameters)
        {
            var typeSymbol = semanticModel.GetSymbolInfo(parameter.Type!).Symbol;
            if (typeSymbol?.ToString() == TypeConstants.ClientRequestOptions)
            {
                clientOptions.Append($"\n            .WithRequestOptions({parameter.Identifier.ValueText})");
                requestOptions.Append($"\n            .WithRequestOptions({parameter.Identifier.ValueText})");
                continue;
            }

            if (typeSymbol?.ToString() == TypeConstants.CancellationToken)
            {
                cancellationToken = parameter.Identifier.ValueText;
                continue;
            }

            foreach (var attList in parameter.AttributeLists)
            {
                foreach (var att in attList.Attributes)
                {
                    var attribute = semanticModel.GetSymbolInfo(att).Symbol?.ContainingType?.ToString();
                    if (attribute == TypeConstants.BodyAttribute)
                    {
                        requestOptions.Append($"\n            .WithBody({parameter.Identifier.ValueText})");
                        continue;
                    }

                    var argValue = att.ArgumentList?.Arguments.FirstOrDefault()?.ToFullString()
                        ?? $"\"{parameter.Identifier.ValueText}\"";

                    if (attribute == TypeConstants.PathAttribute)
                    {
                        requestOptions.Append($"\n            .WithPathParameter({argValue}, {parameter.Identifier.ValueText})");
                        continue;
                    }

                    if (attribute == TypeConstants.QueryAttribute)
                    {
                        requestOptions.Append($"\n            .WithQueryParameter({argValue}, {parameter.Identifier.ValueText})");
                        continue;
                    }

                    if (attribute == TypeConstants.HeaderAttribute)
                    {
                        requestOptions.Append($"\n            .WithHeader({argValue}, {parameter.Identifier.ValueText})");
                        continue;
                    }
                }
            }
        }

        var returnTypeConversion = returnTypeString == null
            ? null
            : $"\n                response => new EndpointResponse{returnTypeString}(response),\n                ";

        source.AppendLine($$"""

            public async {{method.ReturnType}} {{method.Identifier}}{{method.ParameterList}}
            {
                var client = factory.CreateClient("{{clientName}}");

                using var requestMessage = requestFactory
                    .FromTemplate("{{clientName}}", {{routeTemplate}}){{requestOptions}}
                    .Build(HttpMethod.{{httpMethod}});

                using var response = await client{{clientOptions}}
                    .SendAsync(requestMessage, {{cancellationToken}});

                return await requestFactory
                    .FromResponse("{{clientName}}", response)
                    .AddSuccessResponse{{returnTypeString}}(HttpStatusCode.OK)
                    .GetAsync({{returnTypeConversion}}{{cancellationToken}});
            }
        """);
    }

    private static bool IsValidEndpointReturnType(MethodDeclarationSyntax method, out TypeSyntax? returnType)
    {
        returnType = null;
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
            returnType = g.TypeArgumentList.Arguments[0];
        }

        return true;
    }
}


