using Cabazure.Client.SourceGenerator.Descriptors;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cabazure.Client.SourceGenerator;

public class ClientInitializationSyntaxReceiver : ISyntaxContextReceiver
{
    private readonly List<EndpointReferenceDescriptor> endpoints = [];
    private readonly List<InitializationDescriptor> initializations = [];
    private readonly List<Diagnostic> diagnostics = [];

    public IEnumerable<EndpointReferenceDescriptor> Endpoints => endpoints;

    public IEnumerable<InitializationDescriptor> Initializations => initializations;

    public IEnumerable<Diagnostic> Diagnostics => diagnostics;

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
        else if (attributeType == TypeConstants.ClientInitializationAttribute)
        {
            var initialization = InitializationDescriptor.Create(
                diagnostics.Add,
                context.SemanticModel,
                attribute);

            if (initialization != null)
            {
                initializations.Add(initialization);
            }
        }
    }
}
