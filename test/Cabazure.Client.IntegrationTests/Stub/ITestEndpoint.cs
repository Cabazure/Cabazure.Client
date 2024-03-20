namespace Cabazure.Client.IntegrationTests;

[ClientEndpoint("TestStub")]
public interface ITestEndpoint
{
    [Get("/items")]
    Task<EndpointResponse<string[]>> ExecuteAsync(
        CancellationToken cancellationToken);
}