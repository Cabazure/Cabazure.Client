﻿using System.Diagnostics.CodeAnalysis;

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

        /// <param name="routeTemplate">The relative request path which might include placeholders, like: <c>/users/{id}</c>.</param>
        public PutAttribute(
            string routeTemplate)
        {
            RouteTemplate = routeTemplate;
        }
    }
}
