﻿using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cabazure.Client.SourceGenerator;

[Generator]
public class ClientEndpointGenerator : IIncrementalGenerator
{
    private const string ClientEndpointAttribute = $"Cabazure.Client.ClientEndpointAttribute";
    private const string ClientRequestOptions = "Cabazure.Client.ClientRequestOptions";
    private const string GetAttribute = "Cabazure.Client.GetAttribute";
    private const string PostAttribute = "Cabazure.Client.PostAttribute";
    private const string QueryAttribute = "Cabazure.Client.QueryAttribute";
    private const string PathAttribute = "Cabazure.Client.PathAttribute";
    private const string BodyAttribute = "Cabazure.Client.BodyAttribute";
    private const string HeaderAttribute = "Cabazure.Client.HeaderAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var endpointsToGenerate = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                ClientEndpointAttribute,
                static (s, _) => s is InterfaceDeclarationSyntax,
                static (ctx, _) => (TargetNode: (InterfaceDeclarationSyntax)ctx.TargetNode, ctx.SemanticModel));

        context.RegisterImplementationSourceOutput(
            endpointsToGenerate,
            (c, s) => ProcessEndpoint(c, s.TargetNode, s.SemanticModel));
    }

    private static void ProcessEndpoint(
        SourceProductionContext context,
        InterfaceDeclarationSyntax endpointSyntax,
        SemanticModel semanticModel)
    {
        try
        {
            GenerateEndpoint(context, endpointSyntax, semanticModel);
        }
        catch (InvalidOperationException ex)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "CLIENT0001",
                        "Cabazure.Client",
                        "Failed to generate code: " + ex.Message,
                        "ClientEndpointGenerator",
                        DiagnosticSeverity.Error,
                        true),
                    Location.None));
        }
    }

    private static void GenerateEndpoint(
        SourceProductionContext context,
        InterfaceDeclarationSyntax endpointSyntax,
        SemanticModel semanticModel)
    {
        var clientName = endpointSyntax.AttributeLists
            .SelectMany(l => l.Attributes)
            .Where(a => semanticModel.GetSymbolInfo(a).Symbol?.ContainingType.ToString() == ClientEndpointAttribute)
            .Select(a => semanticModel.GetConstantValue(a.ArgumentList!.Arguments[0].Expression).Value)
            .OfType<string>()
            .FirstOrDefault();

        var interfaceName = endpointSyntax.Identifier.ValueText;

        var className = interfaceName.Length > 1 && interfaceName[0] == 'I'
            ? interfaceName.Substring(1)
            : interfaceName + "_Implementation";

        var source = new StringBuilder();
        source.AppendLine("// <auto-generated/>");

        var root = endpointSyntax.SyntaxTree.GetCompilationUnitRoot();
        if (root.Usings.Count > 0)
        {
            source.AppendLine($"{root.Usings}");
        }

        source.AppendLine("""
            using System.Net;
            using Cabazure.Client;
            using Cabazure.Client.Builder;

            """);

        var ns = root
            .ChildNodes()
            .OfType<BaseNamespaceDeclarationSyntax>()
            .Select(ns => ns.Name)
            .FirstOrDefault();
        if (ns != null)
        {
            source.AppendLine($"namespace {ns};\n");
        }

        source.AppendLine($$"""
            public partial class {{className}} : {{interfaceName}}
            {
                private readonly IHttpClientFactory factory;
                private readonly IMessageRequestFactory requestFactory;

                public {{className}}(
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
                clientName,
                semanticModel);
        }

        source.AppendLine("}");

        context.AddSource(
            $"{className}.g.cs",
            source.ToString());
    }

    private static void GenerateEndpointMethod(
        SourceProductionContext context,
        StringBuilder source,
        MethodDeclarationSyntax method,
        string clientName,
        SemanticModel semanticModel)
    {
        if (method.ReturnType is not GenericNameSyntax { } taskType || taskType.Identifier.ToString() == "System.Threading.Task")
        {
            throw new InvalidOperationException("Return type must be either Task<EndpointResponse> or Task<EndpointResponse<T>>");
        }

        if (taskType.TypeArgumentList.Arguments[0] is not SimpleNameSyntax endpointType || endpointType.Identifier.ToString() == "Cabazure.Client.EndpointResponse")
        {
            throw new InvalidOperationException("Return type must be either Task<EndpointResponse> or Task<EndpointResponse<T>>");
        }

        string? returnType = null;
        if (endpointType is GenericNameSyntax g)
        {
            returnType = $"<{g.TypeArgumentList.Arguments[0]}>";
        }

        string httpMethod = nameof(HttpMethod.Get);
        string routeTemplate = "/";
        foreach (var attList in method.AttributeLists)
        {
            foreach (var att in attList.Attributes)
            {
                var attribute = semanticModel.GetSymbolInfo(att).Symbol?.ContainingType?.ToString();
                if (attribute == GetAttribute)
                {
                    httpMethod = nameof(HttpMethod.Get);
                    routeTemplate = att.ArgumentList!.Arguments[0].ToFullString();
                    continue;
                }

                if (attribute == PostAttribute)
                {
                    httpMethod = nameof(HttpMethod.Post);
                    routeTemplate = att.ArgumentList!.Arguments[0].ToFullString();
                    continue;
                }
            }
        }

        var clientOptions = new StringBuilder();
        var requestOptions = new StringBuilder();
        foreach (var parameter in method.ParameterList.Parameters)
        {
            var typeSymbol = semanticModel.GetSymbolInfo(parameter.Type!).Symbol;
            if (typeSymbol?.ToString() == ClientRequestOptions)
            {
                clientOptions.Append($"\n            .WithRequestOptions({parameter.Identifier.ValueText})");
                requestOptions.Append($"\n            .WithRequestOptions({parameter.Identifier.ValueText})");
                continue;
            }

            if (typeSymbol?.ToString() == "System.Threading.CancellationToken")
            {
                continue;
            }

            foreach (var attList in parameter.AttributeLists)
            {
                foreach (var att in attList.Attributes)
                {
                    var attribute = semanticModel.GetSymbolInfo(att).Symbol?.ContainingType?.ToString();
                    if (attribute == BodyAttribute)
                    {
                        requestOptions.Append($"\n            .WithBody({parameter.Identifier.ValueText})");
                        continue;
                    }

                    var argValue = att.ArgumentList?.Arguments.FirstOrDefault()?.ToFullString()
                        ?? $"\"{parameter.Identifier.ValueText}\"";

                    if (attribute == PathAttribute)
                    {
                        requestOptions.Append($"\n            .WithPathParameter({argValue}, {parameter.Identifier.ValueText})");
                        continue;
                    }

                    if (attribute == QueryAttribute)
                    {
                        requestOptions.Append($"\n            .WithQueryParameter({argValue}, {parameter.Identifier.ValueText})");
                        continue;
                    }

                    if (attribute == HeaderAttribute)
                    {
                        requestOptions.Append($"\n            .WithHeader({argValue}, {parameter.Identifier.ValueText})");
                        continue;
                    }
                }
            }
        }

        var returnTypeConversion = returnType == null
            ? null
            : $"\n                response => new EndpointResponse{returnType}(response),\n                ";

        source.AppendLine($$"""

            public async {{method.ReturnType}} {{method.Identifier}}{{method.ParameterList}}
            {
                var client = factory.CreateClient("{{clientName}}");

                using var requestMessage = requestFactory
                    .FromTemplate("{{clientName}}", {{routeTemplate}}){{requestOptions}}
                    .Build(HttpMethod.{{httpMethod}});

                using var response = await client{{clientOptions}}
                    .SendAsync(requestMessage, cancellationToken);

                return await requestFactory
                    .FromResponse("{{clientName}}", response)
                    .AddSuccessResponse{{returnType}}(HttpStatusCode.OK)
                    .GetAsync({{returnTypeConversion}}cancellationToken);
            }
        """);
    }
}


