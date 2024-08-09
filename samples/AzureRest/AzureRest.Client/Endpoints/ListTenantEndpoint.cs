using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AzureRest.Contracts;
using Cabazure.Client;
using Cabazure.Client.Builder;

namespace AzureRest.Client.Endpoints
{
    internal class ListTenantEndpoint : IListTenantsEndpoint
    {
        private readonly IHttpClientFactory factory;
        private readonly IMessageRequestFactory requestFactory;

        public ListTenantEndpoint(
            IHttpClientFactory factory,
            IMessageRequestFactory requestFactory)
        {
            this.factory = factory;
            this.requestFactory = requestFactory;
        }

        public async Task<EndpointResponse<ListResponse<Tenant>>> ExecuteAsync(
            CancellationToken cancellationToken = default)
        {
            var client = factory.CreateClient("azure-rest-client");

            using var requestMessage = requestFactory
                .FromTemplate("azure-rest-client", "tenants?api-version=2022-12-01")
                .Build(HttpMethod.Get);

            using var response = await client
                .SendAsync(requestMessage, cancellationToken);

            return await requestFactory
                .FromResponse("azure-rest-client", response)
                .AddSuccessResponse<ListResponse<Tenant>>(HttpStatusCode.OK)
                .GetAsync(
                    response => new EndpointResponse<ListResponse<Tenant>>(response),
                    cancellationToken);
        }
    }
}
