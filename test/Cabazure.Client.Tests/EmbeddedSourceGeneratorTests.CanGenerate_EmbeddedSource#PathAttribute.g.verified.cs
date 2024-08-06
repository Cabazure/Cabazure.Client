﻿//HintName: PathAttribute.g.cs
// <auto-generated/>
#nullable enable
using System.Diagnostics.CodeAnalysis;

namespace Cabazure.Client;

/// <summary>
/// Specifies that a parameter should be added to a placeholder in the request path.
/// </summary>
/// <param name="name">The name of the placeholder in the path. If obmitted, the parameter name will be used.</param>
[ExcludeFromCodeCoverage]   
[AttributeUsage(
    AttributeTargets.Parameter | AttributeTargets.Property,
    Inherited = false,
    AllowMultiple = false)]
internal sealed class PathAttribute(
    string? name = null,
    string? formatString = null)
    : Attribute
{
    public string? Name { get; } = name;

    public string? FormatString { get; } = formatString;
}
#nullable disable