namespace Cabazure.Client.IntegrationTests;

[ClientEndpoint("Test")]
public interface ITestEndpoint
{
    [Get("/items")]
    public Task<EndpointResponse<string[]>> ExecuteAsync(
        CancellationToken cancellationToken);
}