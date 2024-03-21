namespace Cabazure.Client.Tests;

public class ClientInitializationGeneratorTests
{
    [Fact]
    public Task CanGenerate_GetEndpoint()
        => TestHelper.VerifyInitialization("""
            [ClientEndpoint("ClientName")]
            public interface ITestEndpoint
            {
                [Get("/items")]
                public Task<EndpointResponse<string[]>> ExecuteAsync(
                    ClientRequestOptions options,
                    CancellationToken cancellationToken);
            }
            """);
}
