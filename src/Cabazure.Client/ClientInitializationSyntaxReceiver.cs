﻿using Cabazure.Client.SourceGenerator.Descriptors;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cabazure.Client.SourceGenerator;

public sealed class ClientInitializationSyntaxReceiver : ISyntaxContextReceiver
{
    private readonly List<EndpointReferenceDescriptor> endpoints = [];
    private readonly List<Diagnostic> diagnostics = [];

    public IEnumerable<EndpointReferenceDescriptor> Endpoints => endpoints;

    public IEnumerable<Diagnostic> Diagnostics => diagnostics;

    public bool IsInitializationCalled { get; set; }

    void ISyntaxContextReceiver.OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        if (context.Node is not AttributeSyntax attribute)
        {
            return;
        }

        var attributeType = context.SemanticModel
            .GetSymbolInfo(attribute).Symbol?
            .ContainingType
            .ToString();

        if (attributeType == TypeConstants.ClientEndpointAttribute)
        {
            var endpoint = EndpointReferenceDescriptor.Create(
                diagnostics.Add,
                context.SemanticModel,
                attribute);

            if (endpoint != null)
            {
                endpoints.Add(endpoint);
            }
        }
    }
}
