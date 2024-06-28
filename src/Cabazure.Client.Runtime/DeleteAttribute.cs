using System.Diagnostics.CodeAnalysis;

namespace Cabazure.Client;

/// <summary>
/// Identifies a HTTP DELETE request call.
/// </summary>
/// <param name="routeTemplate">The relative request path which might include placeholders, like: <c>/users/{id}</c>.</param>
[ExcludeFromCodeCoverage]
[AttributeUsage(
    AttributeTargets.Method,
    Inherited = false,
    AllowMultiple = false)]
public sealed class DeleteAttribute(
    string routeTemplate) : Attribute
{
    public string RouteTemplate { get; } = routeTemplate;
}
