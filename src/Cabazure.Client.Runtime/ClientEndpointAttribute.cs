using System.Diagnostics.CodeAnalysis;

namespace Cabazure.Client;

[ExcludeFromCodeCoverage]
[AttributeUsage(
    AttributeTargets.Interface,
    Inherited = false,
    AllowMultiple = false)]
public sealed class ClientEndpointAttribute(
    string clientName)
    : Attribute
{
    public string ClientName { get; } = clientName;
}
