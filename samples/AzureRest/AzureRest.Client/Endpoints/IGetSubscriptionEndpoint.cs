using System.Threading;
using System.Threading.Tasks;
using AzureRest.Contracts;
using Cabazure.Client;

namespace AzureRest.Client.Endpoints
{
    [ClientEndpoint("azure-rest-client")]
    public interface IGetSubscriptionEndpoint
    {
        [Get("subscriptions/{subscriptionId}?api-version=2022-12-01")]
        public Task<EndpointResponse<Subscription>> ExecuteAsync(
            [Path] string subscriptionId,
            CancellationToken cancellationToken = default);
    }
}
