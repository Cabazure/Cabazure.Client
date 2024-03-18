namespace Cabazure.Client.Builder;

public class MessageRequestFactory(IClientSerializer serializer) : IMessageRequestFactory
{
    public IMessageResponseBuilder FromResponse(string clientName, HttpResponseMessage? response)
        => new MessageResponseBuilder(response, serializer, clientName);

    public IMessageRequestBuilder FromTemplate(string clientName, string template)
        => new MessageRequestBuilder(template, serializer, clientName);
}