namespace Cabazure.Client.IntegrationTests.Stub;

[ClientEndpoint("TestStub")]
public interface ITestEndpoint
{
    [Get("/items")]
    Task<EndpointResponse<string[]>> ExecuteAsync(
        CancellationToken cancellationToken);
}