using System.Diagnostics.CodeAnalysis;

namespace Cabazure.Client;

/// <summary>
/// Specifies that a parameter should be bound to a request header.
/// </summary>
/// <param name="name">The name of the request header.</param>
[ExcludeFromCodeCoverage]
[AttributeUsage(
    AttributeTargets.Parameter | AttributeTargets.Property,
    Inherited = false,
    AllowMultiple = false)]
public sealed class HeaderAttribute(
    string name)
    : Attribute
{
    public string Name { get; } = name;
}
