namespace Cabazure.Client.SourceGenerator.Tests;

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
            """,
            """
            using System.Text.Json;
            using Cabazure.Client;

            namespace Microsoft.Extensions.DependencyInjection;

            public static partial class ServiceCollectionExtensions
            {
                [ClientInitialization("ClientName")]
                static partial void ClientInitialization(
                    this IServiceCollection services,
                    Action<IHttpClientBuilder> builder,
                    Action<JsonSerializerOptions> jsonOptions);
            }
            """);
}
