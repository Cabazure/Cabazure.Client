namespace Cabazure.Client;

[AttributeUsage(AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
public sealed class ClientEndpointAttribute(string clientName) : Attribute
{
    public string ClientName { get; } = clientName;
}
