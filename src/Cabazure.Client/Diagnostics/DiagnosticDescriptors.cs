using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace Cabazure.Client.SourceGenerator.Diagnostics;

[SuppressMessage(
    "MicrosoftCodeAnalysisReleaseTracking",
    "RS2008:Enable analyzer release tracking",
    Justification = "Not relevant yet")]
public static class DiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor UnsupportedEndpointReturnType = new(
        "ECL001",
        "ClientEndpoint has unsupported return type",
        "{0}.{1} has an unsupported return type. Return type must be either Task<EndpointResponse>, Task<EndpointResponse<T>> or Task<PagedResponse<T>>.",
        DiagnosticCategories.EndpointGenerator,
        DiagnosticSeverity.Error,
        true
    );

    public static readonly DiagnosticDescriptor EndpointHasNoMethods = new(
        "ECL002",
        "ClientEndpoint definition has no methods",
        "{0} has no methods defined",
        DiagnosticCategories.EndpointGenerator,
        DiagnosticSeverity.Error,
        true
    );

    public static readonly DiagnosticDescriptor MissingEndpointRoute = new(
        "ECL003",
        "ClientEndpoint does not specify route",
        "{0}.{1} does not have a route defined. Please add one of the [Get], [Post], [Put] or [Delete] attributes.",
        DiagnosticCategories.EndpointGenerator,
        DiagnosticSeverity.Error,
        true
    );

    public static readonly DiagnosticDescriptor ClientNameIsEmpty = new(
        "ECL004",
        "ClientName is empty",
        "{0} is defining an empty ClientName",
        DiagnosticCategories.EndpointGenerator,
        DiagnosticSeverity.Error,
        true
    );

    public static readonly DiagnosticDescriptor UnsupportedEndpointParameter = new(
        "ECL005",
        "ClientEndpoint has unsupported parameter",
        "{0}.{1} has an unsupported parameter {2}. Please decorate with either [Body], [Path], [Query] or [Header] attribute.",
        DiagnosticCategories.EndpointGenerator,
        DiagnosticSeverity.Error,
        true
    );

    public static readonly DiagnosticDescriptor PathParameterNotInRouteTemplate = new(
        "ECL006",
        "ClientEndpoint has unused path parameter",
        "{0}.{1} has an unused path parameter {2}. The route template \"{3}\" does not contain a \"{{{4}}}\" placeholder.",
        DiagnosticCategories.EndpointGenerator,
        DiagnosticSeverity.Error,
        true
    );

    public static readonly DiagnosticDescriptor RouteTemplateHasUnmappedPathParameter = new(
        "ECL007",
        "ClientEndpoint is missing a path parameter for the specified route template",
        "{0}.{1} has no path parameter specified for the \"{{{2}}}\" placeholder in the route template \"{3}\"",
        DiagnosticCategories.EndpointGenerator,
        DiagnosticSeverity.Error,
        true
    );
}
