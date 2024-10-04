namespace Cabazure.Client.Builder
{
    public interface IMessageRequestFactory
    {
        IMessageRequestBuilder FromTemplate(string clientName, string template);

        IMessageResponseBuilder FromResponse(string clientName, HttpResponseMessage? response);
    }
}
