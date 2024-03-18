namespace Cabazure.Client;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class GetAttribute(string routeTemplate) : Attribute
{
    public string RouteTemplate { get; } = routeTemplate;
}
