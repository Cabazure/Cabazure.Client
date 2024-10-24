﻿using System.Diagnostics.CodeAnalysis;

namespace Cabazure.Client
{
    /// <summary>
    /// Specifies that a parameter should be added to a placeholder in the request path.
    /// </summary>
    [ExcludeFromCodeCoverage]   
    [AttributeUsage(
        AttributeTargets.Parameter | AttributeTargets.Property,
        Inherited = false,
        AllowMultiple = false)]
    public sealed class PathAttribute : Attribute
    {
        public string? Name { get; }

        public string? FormatString { get; }

        /// <param name="name">The name of the placeholder in the path. If obmitted, the parameter name will be used.</param>
        public PathAttribute(
            string? name = null,
            string? formatString = null)
        {
            Name = name;
            FormatString = formatString;
        }
    }
}
