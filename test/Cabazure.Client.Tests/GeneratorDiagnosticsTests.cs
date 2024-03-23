using Cabazure.Client.SourceGenerator.Diagnostics;
using FluentAssertions;

namespace Cabazure.Client.Tests;

public class GeneratorDiagnosticsTests
{
    [Fact]
    public void ReportDiagnostics_When_EndpointHasNoMethods()
        => TestHelper
            .GetDiagnostics("""
                [ClientEndpoint("ClientName")]
                public interface ITestEndpoint
                {
                }
                """)
            .Should()
            .Contain(DiagnosticDescriptors.EndpointHasNoMethods);

    [Fact]
    public void ReportDiagnostics_When_ClientNameIsEmpty()
    => TestHelper
            .GetDiagnostics("""
                [ClientEndpoint(" ")]
                public interface ITestEndpoint
                {
                    [Get("/items")]
                    public Task<EndpointResponse<string[]>> ExecuteAsync();
                }
                """)
            .Should()
            .Contain(DiagnosticDescriptors.ClientNameIsEmpty);

    [Fact]
    public void ReportDiagnostics_When_ClientNameIsEmpty_UsingAConstant()
        => TestHelper
                .GetDiagnostics("""
                        [ClientEndpoint(Constants.ClientName)]
                        public interface ITestEndpoint
                        {
                            [Get("/items")]
                            public Task<EndpointResponse<string[]>> ExecuteAsync();
                        }
                        """, """
                        public static class Constants 
                        {
                            public const string ClientName = "";
                        }
                        """)
                .Should()
                .Contain(DiagnosticDescriptors.ClientNameIsEmpty);

    [Fact]
    public void ReportDiagnostics_When_MissingEndpointRoute()
    => TestHelper
            .GetDiagnostics("""
                [ClientEndpoint("ClientName")]
                public interface ITestEndpoint
                {
                    public Task<EndpointResponse<string[]>> ExecuteAsync();
                }
                """)
            .Should()
            .Contain(DiagnosticDescriptors.MissingEndpointRoute);

    [Fact]
    public void ReportDiagnostics_When_UnsupportedEndpointReturnType()
        => TestHelper
            .GetDiagnostics("""
                [ClientEndpoint("ClientName")]
                public interface ITestEndpoint
                {
                    [Get("/items")]
                    public string[] ExecuteAsync();
                }
                """)
            .Should()
            .Contain(DiagnosticDescriptors.UnsupportedEndpointReturnType);

    [Fact]
    public void ReportDiagnostics_When_UnsupportedEndpointParameterType()
        => TestHelper
            .GetDiagnostics("""
                [ClientEndpoint("ClientName")]
                public interface ITestEndpoint
                {
                    [Get("/items")]
                    public Task<EndpointResponse<string[]>> ExecuteAsync(
                        DateTimeOffset date);
                }
                """)
            .Should()
            .Contain(DiagnosticDescriptors.UnsupportedEndpointParameter);

    [Fact]
    public void ReportDiagnostics_When_PathParameterNotInRouteTemplate()
        => TestHelper
            .GetDiagnostics("""
                [ClientEndpoint("ClientName")]
                public interface ITestEndpoint
                {
                    [Put("/items/{itemId}")]
                    public Task<EndpointResponse> ExecuteAsync(
                        [Path("id")] string id,
                        [Body] string body,
                        CancellationToken cancellationToken);
                }
                """)
            .Should()
            .Contain(DiagnosticDescriptors.PathParameterNotInRouteTemplate);

    [Fact]
    public void ReportDiagnostics_When_RouteTemplateHasUnmappedPathParameter()
        => TestHelper
            .GetDiagnostics("""
                [ClientEndpoint("ClientName")]
                public interface ITestEndpoint
                {
                    [Put("/items/{id}")]
                    public Task<EndpointResponse> ExecuteAsync(
                        [Body] string body,
                        CancellationToken cancellationToken);
                }
                """)
            .Should()
            .Contain(DiagnosticDescriptors.RouteTemplateHasUnmappedPathParameter);
}
