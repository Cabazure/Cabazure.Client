using System.Threading;
using System.Threading.Tasks;
using AzureRest.Contracts;
using Cabazure.Client;

namespace AzureRest.Client.Endpoints
{
    [ClientEndpoint("azure-rest-client")]
    public interface IListSubscriptionsEndpoint
    {
        [Get("subscriptions?api-version=2022-12-01")]
        public Task<EndpointResponse<ListResponse<Subscription>>> ExecuteAsync(
            CancellationToken cancellationToken = default);
    }
}
