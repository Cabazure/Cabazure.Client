using System.Threading;
using System.Threading.Tasks;
using AzureRest.Contracts;
using Cabazure.Client;

namespace AzureRest.Client.Endpoints
{
    public interface IListTenantsEndpoint
    {
        Task<EndpointResponse<ListResponse<Tenant>>> ExecuteAsync(
            CancellationToken cancellationToken = default);
    }
}
