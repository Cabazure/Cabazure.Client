using System.Diagnostics.CodeAnalysis;

namespace Cabazure.Client
{
    /// <summary>
    /// Identifies a HTTP POST request call.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [AttributeUsage(
        AttributeTargets.Method, 
        Inherited = false, 
        AllowMultiple = false)]
    public sealed class PostAttribute : Attribute
    {
        public string RouteTemplate { get; }

        /// <param name="routeTemplate">The relative request path which might include placeholders, like: <c>/users/{id}</c>.</param>
        public PostAttribute(
            string routeTemplate)
        {
            RouteTemplate = routeTemplate;
        }
    }
}
