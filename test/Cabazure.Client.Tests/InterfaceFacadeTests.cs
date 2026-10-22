namespace Cabazure.Client.Tests;

public class InterfaceFacadeTests
{
    private const string EndpointSource = """
        [ClientEndpoint("ClientName")]
        public interface ITestEndpoint
        {
            public Task<EndpointResponse> ExecuteAsync(
                string id,
                bool refresh,
                ClientRequestOptions options,
                CancellationToken cancellationToken)
                => SendAsync(id, GetCacheControl(refresh), options, cancellationToken);

            public Task<EndpointResponse> ExecuteAsync(
                string id,
                ClientRequestOptions options,
                CancellationToken cancellationToken)
            {
                return ExecuteAsync(id, false, options, cancellationToken);
            }

            private string? GetCacheControl(bool refresh)
                => refresh ? "no-cache" : null;

            [Get("/items/{id}")]
            internal Task<EndpointResponse> SendAsync(
                [Path] string id,
                [Header("Cache-Control")] string? cacheControl,
                ClientRequestOptions options,
                CancellationToken cancellationToken);
        }
        """;

    [Fact]
    public Task CanGenerate_InternalEndpoint_With_DefaultFacades()
        => TestHelper.VerifyEndpoint(EndpointSource);

    [Fact]
    public Task CanGenerate_Initialization_With_DefaultMethodsOnly()
        => TestHelper.VerifyInitialization("""
            [ClientEndpoint("ClientName")]
            public interface ITestEndpoint
            {
                public string Describe(bool refresh) => refresh ? "refresh" : "get";
            }
            """);

    [Fact]
    public Task CanGenerate_Implementation_With_DefaultMethodsOnly()
        => TestHelper.VerifyEndpoint("""
            [ClientEndpoint("ClientName")]
            public interface ITestEndpoint
            {
                public string Describe(bool refresh) => refresh ? "refresh" : "get";
            }
            """);

    [Fact]
    public Task CanIgnore_AttributedDefaultMethods()
        => TestHelper.VerifyEndpoint("""
            [ClientEndpoint("ClientName")]
            public interface ITestEndpoint
            {
                [Get("/unused/{unmapped}")]
                public string Describe(bool refresh) => refresh ? "refresh" : "get";

                [Post("/unused")]
                public int Describe(int value)
                {
                    return value;
                }

                [Get("/items")]
                internal Task<EndpointResponse> SendAsync();
            }
            """);

    [Fact]
    public void Should_Not_Report_Diagnostics_For_DefaultMethods()
        => TestHelper.GetDiagnostics(EndpointSource)
            .Should()
            .BeEmpty();

    [Fact]
    public void Should_Still_Report_MissingRoute_For_AbstractMethods()
        => TestHelper.GetDiagnostics("""
            [ClientEndpoint("ClientName")]
            public interface ITestEndpoint
            {
                public string Describe() => "items";

                internal Task<EndpointResponse> SendAsync();
            }
            """)
            .Should()
            .Contain(SourceGenerator.Diagnostics.DiagnosticDescriptors.MissingEndpointRoute);

    [Fact]
    public void Should_Expose_Facade_To_AnotherAssembly()
        => TestHelper.GetConsumerErrors(EndpointSource, """
            public class Consumer
            {
                public Task<EndpointResponse> ExecuteAsync(ITestEndpoint endpoint)
                    => endpoint.ExecuteAsync("123", true, new ClientRequestOptions(), CancellationToken.None);
            }
            """)
            .Should()
            .BeEmpty();

    [Fact]
    public void Should_Hide_TransportMethod_From_AnotherAssembly()
    {
        var errors = TestHelper.GetConsumerErrors(EndpointSource, """
            public class Consumer
            {
                public Task<EndpointResponse> ExecuteAsync(ITestEndpoint endpoint)
                    => endpoint.SendAsync("123", "no-cache", new ClientRequestOptions(), CancellationToken.None);
            }
            """);

        errors.Should().ContainSingle()
            .Which.Id.Should().Be("CS0122");
    }
}
