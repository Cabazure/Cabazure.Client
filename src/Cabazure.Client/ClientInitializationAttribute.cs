namespace Cabazure.Client;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class ClientInitializationAttribute(string clientName) : Attribute
{
    public string ClientName { get; } = clientName;
}
