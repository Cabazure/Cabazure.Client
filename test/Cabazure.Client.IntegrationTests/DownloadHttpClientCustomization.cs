using AutoFixture;

namespace Cabazure.Client.IntegrationTests;

/// <summary>
/// Overrides <see cref="AutoFixtureCustomization"/>'s <see cref="HttpClient"/> creation for
/// stream-response tests, which need a real <see cref="HttpClient"/> (rather than a substitute)
/// so that the non-virtual <c>SendAsync(request, completionOption, cancellationToken)</c>
/// overload used for stream endpoints is actually exercised, instead of falling through to
/// real networking. The inner <see cref="HttpMessageHandler"/> is substituted instead, since
/// its <c>SendAsync</c> is the extension point <see cref="HttpClient"/> always dispatches to.
/// </summary>
public class DownloadHttpClientCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        fixture
            .Customize<HttpMessageHandler>(c => c
            .FromFactory((IFixture f) => CreateHandler(f)));

        fixture
            .Customize<HttpClient>(c => c
            .FromFactory((IFixture f) => new HttpClient(f.Create<HttpMessageHandler>())));
    }

    private static HttpMessageHandler CreateHandler(IFixture fixture)
    {
        var handler = Substitute.For<HttpMessageHandler>();
        handler
            .InvokeProtectedAsync<HttpResponseMessage>(
                "SendAsync",
                Arg.Any<HttpRequestMessage>(),
                Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(c => fixture.Create<HttpResponseMessage>());

        return handler;
    }
}
