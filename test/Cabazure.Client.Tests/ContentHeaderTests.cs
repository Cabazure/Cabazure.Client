using Cabazure.Client.SourceGenerator.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Cabazure.Client.Tests;

public class ContentHeaderTests
{
    [Fact]
    public Task CanGenerate_With_Body_After_ContentHeaders()
        => TestHelper.VerifyEndpoint("""
            [ClientEndpoint("ClientName")]
            public interface ITestEndpoint
            {
                [Post("/items")]
                public Task<EndpointResponse> ExecuteAsync(
                    [Header("Content-Type")] string? contentType,
                    [Header("Content-Language")] string? language,
                    [Body] string body);
            }
            """);

    [Fact]
    public Task CanGenerate_InternalStream_With_Body_Before_ContentHeader()
        => TestHelper.VerifyEndpoint("""
            [ClientEndpoint("ClientName")]
            public interface ITestEndpoint
            {
                [Post("/items")]
                internal Task<StreamResponse> SendAsync(
                    [Body] string body,
                    [Header("Content-Type")] string contentType,
                    CancellationToken cancellationToken);
            }
            """);

    [Theory]
    [InlineData("Allow")]
    [InlineData("Content-Disposition")]
    [InlineData("Content-Encoding")]
    [InlineData("Content-Language")]
    [InlineData("Content-Length")]
    [InlineData("Content-Location")]
    [InlineData("Content-MD5")]
    [InlineData("Content-Range")]
    [InlineData("content-type")]
    [InlineData("Expires")]
    [InlineData("Last-Modified")]
    public void Should_Report_ContentHeader_Without_Body(string name)
    {
        var diagnostics = TestHelper.GetGeneratorDiagnostics($$"""
            [ClientEndpoint("ClientName")]
            public interface ITestEndpoint
            {
                [Get("/items")]
                public Task<EndpointResponse> ExecuteAsync(
                    [Header("{{name}}")] string? header);
            }
            """);

        var diagnostic = diagnostics.Should().ContainSingle().Subject;
        diagnostic.Descriptor.Should().Be(DiagnosticDescriptors.ContentHeaderRequiresBody);
        diagnostic.Severity.Should().Be(DiagnosticSeverity.Error);
        diagnostic.GetMessage().Should().Be(
            $"ITestEndpoint.ExecuteAsync binds content header '{name}' but has no [Body] parameter. Add a [Body] parameter or remove the content header binding.");
        diagnostic.Location.SourceTree!.GetText()
            .ToString(diagnostic.Location.SourceSpan)
            .Should()
            .Be($"[Header(\"{name}\")] string? header");
    }

    [Fact]
    public void Should_Recognize_Constant_ContentHeader_Names()
        => TestHelper.GetDiagnostics("""
            public static class Headers
            {
                public const string Type = "cOnTeNt-TyPe";
            }

            [ClientEndpoint("ClientName")]
            public interface ITestEndpoint
            {
                [Post("/items")]
                public Task<EndpointResponse> ExecuteAsync(
                    [Header(name: Headers.Type)] string contentType);
            }
            """)
            .Should()
            .Contain(DiagnosticDescriptors.ContentHeaderRequiresBody);

    [Fact]
    public void Should_Report_Each_ContentHeader_Without_Body()
        => TestHelper.GetDiagnostics("""
            [ClientEndpoint("ClientName")]
            public interface ITestEndpoint
            {
                [Get("/items")]
                public Task<EndpointResponse> ExecuteAsync(
                    [Header("Content-Type")] string contentType,
                    [Header("Content-Language")] string language);
            }
            """)
            .Should()
            .Equal(
                DiagnosticDescriptors.ContentHeaderRequiresBody,
                DiagnosticDescriptors.ContentHeaderRequiresBody);

    [Fact]
    public void Should_Ignore_ContentHeader_Bindings_On_DefaultMethods()
        => TestHelper.GetDiagnostics("""
            [ClientEndpoint("ClientName")]
            public interface ITestEndpoint
            {
                [Get("/unused")]
                public string Describe([Header("Content-Type")] string contentType)
                    => contentType;
            }
            """)
            .Should()
            .BeEmpty();
}
