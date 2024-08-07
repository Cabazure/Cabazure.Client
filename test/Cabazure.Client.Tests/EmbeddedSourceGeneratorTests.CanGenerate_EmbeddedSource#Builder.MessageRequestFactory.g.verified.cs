﻿//HintName: Builder.MessageRequestFactory.g.cs
// <auto-generated/>
#nullable enable
using System.Net.Http;

namespace Cabazure.Client.Builder
{
    internal class MessageRequestFactory : IMessageRequestFactory
    {
        private readonly IClientSerializer serializer;

        public MessageRequestFactory(IClientSerializer serializer)
        {
            this.serializer = serializer;
        }

        public IMessageResponseBuilder FromResponse(string clientName, HttpResponseMessage? response)
            => new MessageResponseBuilder(response, serializer, clientName);

        public IMessageRequestBuilder FromTemplate(string clientName, string template)
            => new MessageRequestBuilder(template, serializer, clientName);
    }
}
#nullable disable