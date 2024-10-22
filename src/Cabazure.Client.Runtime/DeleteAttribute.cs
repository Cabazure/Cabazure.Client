using System.Diagnostics.CodeAnalysis;

namespace Cabazure.Client
{
    /// <summary>
    /// Identifies a HTTP DELETE request call.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [AttributeUsage(
        AttributeTargets.Method,
        Inherited = false,
        AllowMultiple = false)]
    public sealed class DeleteAttribute : Attribute
    {
        public string RouteTemplate { get; }

        /// <param name="routeTemplate">The relative request path which might include placeholders, like: <c>/users/{id}</c>.</param>
        public DeleteAttribute(
            string routeTemplate)
        {
            RouteTemplate = routeTemplate;
        }
    }
}
