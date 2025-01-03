﻿using System.Diagnostics.CodeAnalysis;

namespace Cabazure.Client
{
    /// <summary>
    /// Specifies that a parameter should be bound to a request header.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [AttributeUsage(
        AttributeTargets.Parameter | AttributeTargets.Property,
        Inherited = false,
        AllowMultiple = false)]
    public sealed class HeaderAttribute : Attribute
    {
        public string Name { get; }

        public string? FormatString { get; }

        /// <param name="name">The name of the request header.</param>
        public HeaderAttribute(
            string name,
            string? formatString = null)
        {
            Name = name;
            FormatString = formatString;
        }
    }
}
