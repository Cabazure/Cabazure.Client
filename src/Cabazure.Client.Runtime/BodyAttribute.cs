namespace Cabazure.Client;

[AttributeUsage(
    AttributeTargets.Parameter | AttributeTargets.Property,
    Inherited = false,
    AllowMultiple = false)]
public sealed class BodyAttribute : Attribute
{
}
