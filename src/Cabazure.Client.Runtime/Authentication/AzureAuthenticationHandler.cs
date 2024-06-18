namespace Cabazure.Client.Authentication;

public class AzureAuthenticationHandler(
    IBearerTokenProvider tokenProvider)
    : DelegatingHandler
{
    protected async override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        request.Headers.Authorization = await tokenProvider.GetTokenAsync(cancellationToken);

        return await base.SendAsync(request, cancellationToken);
    }
}
