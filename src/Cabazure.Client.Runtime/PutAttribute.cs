using System.Diagnostics.CodeAnalysis;

namespace Cabazure.Client
{
    /// <summary>
    /// Identifies a HTTP PUT request call.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [AttributeUsage(
        AttributeTargets.Method,
        Inherited = false,
        AllowMultiple = false)]
    public sealed class PutAttribute : Attribute
    {
        public string RouteTemplate { get; }
        public int[] SuccessStatusCodes { get; }

        /// <param name="routeTemplate">The relative request path which might include placeholders, like: <c>/users/{id}</c>.</param>
        /// <param name="successStatusCodes">HTTP status codes considered successful for this endpoint. Defaults to 200 (OK).</param>
        public PutAttribute(
            string routeTemplate,
            params int[] successStatusCodes)
        {
            RouteTemplate = routeTemplate;
            SuccessStatusCodes = successStatusCodes.Length > 0
                ? successStatusCodes
                : [200];
        }
    }
}
