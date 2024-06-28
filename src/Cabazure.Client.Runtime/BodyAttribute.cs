using System.Diagnostics.CodeAnalysis;

namespace Cabazure.Client;

/// <summary>
/// Specifies that a parameter should be bound to the request body.
/// </summary>
[ExcludeFromCodeCoverage]
[AttributeUsage(
    AttributeTargets.Parameter | AttributeTargets.Property,
    Inherited = false,
    AllowMultiple = false)]
public sealed class BodyAttribute : Attribute
{
}
