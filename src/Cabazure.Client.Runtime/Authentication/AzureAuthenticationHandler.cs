namespace Cabazure.Client.Authentication
{
    public class AzureAuthenticationHandler
        : DelegatingHandler
    {
        private readonly IBearerTokenProvider tokenProvider;

        public AzureAuthenticationHandler(
            IBearerTokenProvider tokenProvider)
        {
            this.tokenProvider = tokenProvider;     
        }

        protected async override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            request.Headers.Authorization = await tokenProvider.GetTokenAsync(cancellationToken);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
