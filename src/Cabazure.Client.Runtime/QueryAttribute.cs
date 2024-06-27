using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Cabazure.Client;

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
