﻿using System.Diagnostics.CodeAnalysis;
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
        "{0} has an unsupported return type. Return type must be either Task<EndpointResponse>, Task<EndpointResponse<T>> or Task<PagedResponse<T>>.",
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

    public static readonly DiagnosticDescriptor MissingEndpointRoute = new(
        "ECL003",
        "ClientEndpoint does not specify route",
        "{0}.{1} does not have a route defined. Please add one of the [Get], [Post], [Put] or [Delete] attributes.",
        DiagnosticCategories.EndpointGenerator,
        DiagnosticSeverity.Error,
        true
    );

    public static readonly DiagnosticDescriptor UnsupportedEndpointType = new(
        "ECL004",
        "ClientEndpoint is not an interface",
        "{0} is not an interface",
        DiagnosticCategories.EndpointGenerator,
        DiagnosticSeverity.Error,
        true
    );

    public static readonly DiagnosticDescriptor UnsupportedInitializationParameter = new(
        "ECL005",
        "ClientInitialization has unsupported parameter",
        "{0} has an unsupported parameter {1}",
        DiagnosticCategories.EndpointGenerator,
        DiagnosticSeverity.Warning,
        true
    );

    public static readonly DiagnosticDescriptor MissingRequiredInitializationParameter = new(
        "ECL006",
        "ClientInitialization is missing required parameter",
        "{0} is missing required parameter {1}",
        DiagnosticCategories.EndpointGenerator,
        DiagnosticSeverity.Error,
        true
    );
}
