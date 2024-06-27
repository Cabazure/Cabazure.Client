using System.Diagnostics.CodeAnalysis;

namespace Cabazure.Client;

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
