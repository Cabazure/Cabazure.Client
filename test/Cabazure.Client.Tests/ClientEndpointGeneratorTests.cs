namespace Cabazure.Client.Tests;

public class ClientEndpointGeneratorTests
{
    [Fact]
    public Task CanGenerate_GetEndpoint()
        => TestHelper.VerifyEndpoint("""
            [ClientEndpoint("ClientName")]
            public interface ITestEndpoint
            {
                [Get("/items")]
                public Task<EndpointResponse<string[]>> ExecuteAsync();
            }
            """);

    [Fact]
    public Task CanGenerate_CancellationToken_Parameter()
        => TestHelper.VerifyEndpoint("""
            [ClientEndpoint("ClientName")]
            public interface ITestEndpoint
            {
                [Get("/items")]
                public Task<EndpointResponse<string[]>> ExecuteAsync(
                    CancellationToken cancellationToken);
            }
            """);

    [Fact]
    public Task CanGenerate_ClientRequestOptions_Parameter()
        => TestHelper.VerifyEndpoint("""
            [ClientEndpoint("ClientName")]
            public interface ITestEndpoint
            {
                [Get("/items")]
                public Task<EndpointResponse<string[]>> ExecuteAsync(
                    ClientRequestOptions options);
            }
            """);

    [Fact]
    public Task CanGenerate_ClientPaginationOptions_Parameter()
        => TestHelper.VerifyEndpoint("""
            [ClientEndpoint("ClientName")]
            public interface ITestEndpoint
            {
                [Get("/items")]
                public Task<EndpointResponse<string[]>> ExecuteAsync(
                    ClientPaginationOptions options);
            }
            """);

    [Fact]
    public Task CanGenerate_PathParameter()
        => TestHelper.VerifyEndpoint("""
            [ClientEndpoint("ClientName")]
            public interface ITestEndpoint
            {
                [Get("/items/{id}")]
                public Task<EndpointResponse<string>> ExecuteAsync(
                    [Path] string id,
                    ClientRequestOptions options,
                    CancellationToken cancellationToken);
            }
            """);

    [Fact]
    public Task CanGenerate_NamedPathParameter()
        => TestHelper.VerifyEndpoint("""
            [ClientEndpoint("ClientName")]
            public interface ITestEndpoint
            {
                [Get("/items/{id}")]
                public Task<EndpointResponse<string>> ExecuteAsync(
                    [Path("id")] string itemId,        
                    ClientRequestOptions options,
                    CancellationToken cancellationToken);
            }
            """);

    [Fact]
    public Task CanGenerate_QueryParameter()
        => TestHelper.VerifyEndpoint("""
            [ClientEndpoint("ClientName")]
            public interface ITestEndpoint
            {
                [Get("/items")]
                public Task<EndpointResponse<string[]>> ExecuteAsync(
                    [Query] string searchText,        
                    ClientRequestOptions options,
                    CancellationToken cancellationToken);
            }
            """);

    [Fact]
    public Task CanGenerate_NamedQueryParameter()
        => TestHelper.VerifyEndpoint("""
            [ClientEndpoint("ClientName")]
            public interface ITestEndpoint
            {
                [Get("/items")]
                public Task<EndpointResponse<string[]>> ExecuteAsync(
                    [Query("s")] string searchText,        
                    ClientRequestOptions options,
                    CancellationToken cancellationToken);
            }
            """);

    [Fact]
    public Task CanGenerate_HeaderParameter()
        => TestHelper.VerifyEndpoint("""
            [ClientEndpoint("ClientName")]
            public interface ITestEndpoint
            {
                [Get("/items")]
                public Task<EndpointResponse<string[]>> ExecuteAsync(
                    [Header("x-correlation-id")] string correlationId,
                    ClientRequestOptions options,
                    CancellationToken cancellationToken);
            }
            """);

    [Fact]
    public Task CanGenerate_PostEndpoint()
        => TestHelper.VerifyEndpoint("""
            [ClientEndpoint("ClientName")]
            public interface ITestEndpoint
            {
                [Post("/routes")]
                public Task<EndpointResponse> ExecuteAsync(
                    [Body] string body,
                    ClientRequestOptions options,
                    CancellationToken cancellationToken);
            }
            """);

    [Fact]
    public Task CanGenerate_Usings()
        => TestHelper.VerifyEndpoint("""
            using System.Text;

            [ClientEndpoint("ClientName")]
            public interface ITestEndpoint
            {
                [Get("/routes")]
                public Task<EndpointResponse<string[]>> ExecuteAsync(
                    ClientRequestOptions options,
                    CancellationToken cancellationToken);
            }
            """);

    [Fact]
    public Task CanGenerate_Namespace()
        => TestHelper.VerifyEndpoint("""
            namespace Test
            {
                [ClientEndpoint("ClientName")]
                public interface ITestEndpoint
                {
                    [Get("/routes")]
                    public Task<EndpointResponse<string[]>> ExecuteAsync(
                        ClientRequestOptions options,
                        CancellationToken cancellationToken);
                }
            }
            """);

    [Fact]
    public Task CanGenerate_FileScopedNamespace()
        => TestHelper.VerifyEndpoint("""
            namespace Test;
            
            [ClientEndpoint("ClientName")]
            public interface ITestEndpoint
            {
                [Get("/routes")]
                public Task<EndpointResponse<string[]>> ExecuteAsync(
                    ClientRequestOptions options,
                    CancellationToken cancellationToken);
            }
            """);

    [Fact]
    public Task CanGenerate_ClientName_From_Constant()
        => TestHelper.VerifyEndpoint("""
            namespace Test;

            [ClientEndpoint(Constants.ClientName)]
            public interface ITestEndpoint
            {
                [Get("/routes")]
                public Task<EndpointResponse<string[]>> ExecuteAsync(
                    ClientRequestOptions options,
                    CancellationToken cancellationToken);
            }
            """, """
            public static class Constants 
            {
                public const string ClientName = "ConstantName";
            }
            """);

    [Fact]
    public Task CanGenerate_ClientName_From_NameOf()
        => TestHelper.VerifyEndpoint("""
            namespace Test;
            
            [ClientEndpoint(nameof(String))]
            public interface ITestEndpoint
            {
                [Get("/routes")]
                public Task<EndpointResponse<string[]>> ExecuteAsync(
                    ClientRequestOptions options,
                    CancellationToken cancellationToken);
            }
            """);

    [Fact]
    public Task CanGenerate_Pagination_Endpoint()
        => TestHelper.VerifyEndpoint("""
            namespace Test;
            
            [ClientEndpoint("ClientName")]
            public interface ITestEndpoint
            {
                [Get("/routes")]
                public Task<PagedResponse<string[]>> ExecuteAsync(
                    ClientPaginationOptions options,
                    CancellationToken cancellationToken);
            }
            """);

    [Fact]
    public Task CanGenerate_PutEndpoint()
        => TestHelper.VerifyEndpoint("""
            [ClientEndpoint("ClientName")]
            public interface ITestEndpoint
            {
                [Put("/items/{id}")]
                public Task<EndpointResponse> ExecuteAsync(
                    [Path("id")] string id,
                    [Body] string body,
                    CancellationToken cancellationToken);
            }
            """);

    [Fact]
    public Task CanGenerate_DeleteEndpoint()
    => TestHelper.VerifyEndpoint("""
            [ClientEndpoint("ClientName")]
            public interface ITestEndpoint
            {
                [Delete("/items/{id}")]
                public Task<EndpointResponse> ExecuteAsync(
                    [Path] string id,
                    CancellationToken cancellationToken);
            }
            """);
}
