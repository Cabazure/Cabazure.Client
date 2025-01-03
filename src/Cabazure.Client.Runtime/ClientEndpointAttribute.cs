﻿using System.Diagnostics.CodeAnalysis;

namespace Cabazure.Client
{
    /// <summary>
    /// Indicates that the interface is a specification for a client endpoint.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [AttributeUsage(
        AttributeTargets.Interface,
        Inherited = false,
        AllowMultiple = false)]
    public sealed class ClientEndpointAttribute : Attribute
    {
        public string ClientName { get; }

        /// <param name="clientName">
        /// The name of the <see cref="HttpClient"/> instance, the endpoint belongs to.
        /// </param>
        public ClientEndpointAttribute(
            string clientName)
        {
            ClientName = clientName;
        }
    }
}
