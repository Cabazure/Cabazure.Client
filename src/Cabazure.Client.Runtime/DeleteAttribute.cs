﻿using System.Diagnostics.CodeAnalysis;

namespace Cabazure.Client;

[ExcludeFromCodeCoverage]
[AttributeUsage(
    AttributeTargets.Method,
    Inherited = false,
    AllowMultiple = false)]
public sealed class DeleteAttribute(
    string routeTemplate) : Attribute
{
    public string RouteTemplate { get; } = routeTemplate;
}
