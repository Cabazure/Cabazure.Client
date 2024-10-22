namespace Cabazure.Client.Builder
{
    public interface IMessageRequestFactory
    {
        IMessageRequestBuilder FromTemplate(string clientName, string routeTemplate);

        IMessageResponseBuilder FromResponse(string clientName, HttpResponseMessage? response);
    }
}
