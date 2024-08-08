﻿using System.Text;
using Cabazure.Client.SourceGenerator.Descriptors;
using Microsoft.CodeAnalysis;

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
            ProcessEndpoint);
    }

    private static void ProcessEndpoint(
        SourceProductionContext context,
        GeneratorAttributeSyntaxContext syntax)
    {
        var endpoint = EndpointDescriptor.Create(
            context.ReportDiagnostic,
            syntax);
        if (endpoint == null)
        {
            return;
        }

        var source = new StringBuilder();
        source.AppendLine("// <auto-generated/>");
        source.AppendLine("#nullable enable");

        var usingStatements = endpoint
            .Usings
            .Append("System.Net")
            .Append("System.Net.Http")
            .Append("Cabazure.Client")
            .Append("Cabazure.Client.Builder")
            .Where(us => us != endpoint.Namespace)
            .Distinct()
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

        if (endpoint.Namespace != null)
        {
            source.AppendLine($"namespace {endpoint.Namespace}");
            source.AppendLine($"{{");
        }

        source.AppendLine($$"""
                internal partial class {{endpoint.ClassName}} : {{endpoint.InterfaceName}}
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

        foreach (var method in endpoint.Methods)
        {
            GenerateEndpointMethod(
                source,
                method,
                endpoint.ClientName);
        }

        source.AppendLine("    }");
        if (endpoint.Namespace != null)
        {
            source.AppendLine($"}}");
        }
        source.AppendLine("#nullable disable");

        context.AddSource(
            $"{endpoint.ClassName}.g.cs",
            source.ToString());
    }

    private static void GenerateEndpointMethod(
        StringBuilder source,
        EndpointMethodDescriptor method,
        string clientName)
    {
        var clientOptions = new StringBuilder();
        var requestOptions = new StringBuilder();

        foreach (var p in method.PathParameters)
        {
            requestOptions.Append($"\n                .WithPathParameter(\"{p.Name}\", {GetParameterValue(p)})");
        }

        foreach (var p in method.QueryParameters)
        {
            requestOptions.Append($"\n                .WithQueryParameter(\"{p.Name}\", {GetParameterValue(p)})");
        }

        foreach (var p in method.HeaderParameters)
        {
            requestOptions.Append($"\n                .WithHeader(\"{p.Name}\", {GetParameterValue(p)})");
        }

        if (method.BodyParameter is { } b)
        {
            requestOptions.Append($"\n                .WithBody({b})");
        }

        if (method.OptionsParameter is { } o)
        {
            clientOptions.Append($"\n                .WithRequestOptions({o})");
            requestOptions.Append($"\n                .WithRequestOptions({o})");
        }

        var cancellationToken = method.CancellationTokenParameter
            ?? "CancellationToken.None";

        string? resultGeneric = null;
        if (method.ResultType is { } rt)
        {
            resultGeneric = $"<{rt}>";
        }

        var resultConversion = method.ResponseType == null
            ? null
            : $"\n                    response => new {method.ResponseType}(response),\n                    ";

        source.AppendLine($$"""

                {{method.Signature}}
                {
                    var client = factory.CreateClient("{{clientName}}");

                    using var requestMessage = requestFactory
                        .FromTemplate("{{clientName}}", "{{method.RouteTemplate}}"){{requestOptions}}
                        .Build(HttpMethod.{{method.HttpMethod}});

                    using var response = await client{{clientOptions}}
                        .SendAsync(requestMessage, {{cancellationToken}});

                    return await requestFactory
                        .FromResponse("{{clientName}}", response)
                        .AddSuccessResponse{{resultGeneric}}(HttpStatusCode.OK)
                        .GetAsync({{resultConversion}}{{cancellationToken}});
                }
        """);
    }

    private static string GetParameterValue(EndpointParameter parameter)
        => parameter switch
        {
            { ParameterType: "System.String" or "string" } => parameter.ParameterName,
            { FormatString: { } f, IsNullable: true } => $"{parameter.ParameterName}?.ToString(\"{f}\")",
            { FormatString: { } f } => $"{parameter.ParameterName}.ToString(\"{f}\")",
            { IsNullable: true } => $"{parameter.ParameterName}?.ToString()",
            _ => $"{parameter.ParameterName}.ToString()",
        };
}


