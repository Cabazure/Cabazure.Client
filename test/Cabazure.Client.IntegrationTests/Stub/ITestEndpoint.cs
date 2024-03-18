namespace Cabazure.Client.IntegrationTests;

[ClientEndpoint("TestStub")]
public interface ITestEndpoint
{
    [Get("/items")]
    public Task<EndpointResponse<string[]>> ExecuteAsync(
        CancellationToken cancellationToken);
}