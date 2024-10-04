namespace Cabazure.Client.Builder
{
    public class MessageRequestFactory : IMessageRequestFactory
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
