using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Cabazure.Client;

/// <summary>
/// Specifies that a parameter should be bound to a request query parameter.
/// </summary>
/// <param name="name">The name of tyhe query parameter. If obmitted, the parameter name will be used.</param>
[ExcludeFromCodeCoverage]
[AttributeUsage(
    AttributeTargets.Parameter | AttributeTargets.Property,
    Inherited = false,
    AllowMultiple = false)]
public sealed class QueryAttribute(
    [CallerMemberName] string? name = null)
    : Attribute
{
    public string? Name { get; } = name;
}
