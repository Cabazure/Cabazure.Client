using Microsoft.CodeAnalysis;

namespace Cabazure.Client.SourceGenerator.Diagnostics;

public static class DiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor UnsupportedEndpointReturnType = new(
        "ECL001",
        "ClientEndpoint has unsupported return type",
        "{0} has an unsupported return type. Return type must be either Task<EndpointResponse> or Task<EndpointResponse<T>>.",
        DiagnosticCategories.EndpointGenerator,
        DiagnosticSeverity.Error,
        true
    );

    public static readonly DiagnosticDescriptor InitializationNotInPartialClass = new(
        "ECL002",
        "ClientInitialization must be defined in a partial class",
        "{0} is not a partial class",
        DiagnosticCategories.EndpointGenerator,
        DiagnosticSeverity.Error,
        true
    );
}
