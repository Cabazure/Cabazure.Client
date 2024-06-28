using Cabazure.Client;
using Microsoft.Graph.Contracts;

namespace Microsoft.Graph.Client.Endpoints;

[ClientEndpoint("ms-graph-client")]
public interface IGetUserByEmail
{
    [Get("v1.0/users/{email}")]
    Task<EndpointResponse<User>> ExecuteAsync(
        [Path] string email,
        CancellationToken cancellationToken = default);
}
