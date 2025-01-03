﻿using System.Diagnostics.CodeAnalysis;

namespace Cabazure.Client
{
    /// <summary>
    /// Specifies that a parameter should be bound to a request query parameter.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [AttributeUsage(
        AttributeTargets.Parameter | AttributeTargets.Property,
        Inherited = false,
        AllowMultiple = false)]
    public sealed class QueryAttribute : Attribute
    {
        public string? Name { get; }

        public string? FormatString { get; }

        /// <param name="name">The name of tyhe query parameter. If obmitted, the parameter name will be used.</param>
        public QueryAttribute(
            string? name = null,
            string? formatString = null)
        {
            Name = name;
            FormatString = formatString;
        }
    }
}
