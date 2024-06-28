using System.Diagnostics.CodeAnalysis;

namespace Cabazure.Client;

/// <summary>
/// Identifies a HTTP PUT request call.
/// </summary>
/// <param name="routeTemplate">The relative request path which might include placeholders, like: <c>/users/{id}</c>.</param>
[ExcludeFromCodeCoverage]
[AttributeUsage(
    AttributeTargets.Method, 
    Inherited = false, 
    AllowMultiple = false)]
public sealed class PutAttribute(
    string routeTemplate)
    : Attribute
{
    public string RouteTemplate { get; } = routeTemplate;
}
