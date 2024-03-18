using System.Runtime.CompilerServices;

namespace Cabazure.Client;

[AttributeUsage(
    AttributeTargets.Parameter | AttributeTargets.Property,
    Inherited = false,
    AllowMultiple = false)]
public sealed class PathAttribute([CallerMemberName] string? name = null) : Attribute
{
    public string? Name { get; } = name;
}
