using System.Net.Http.Headers;
using AutoFixture.AutoNSubstitute;
using Cabazure.Client.Authentication;

namespace Cabazure.Client.Tests.Authentication;

public class AzureAuthenticationHandlerTests
{
    [Theory, AutoNSubstituteData]
    public async Task Should_Get_Token_On_Send(
        [Frozen] IBearerTokenProvider tokenProvider,
        AzureAuthenticationHandler sut,
        [Substitute] HttpMessageHandler handler,
        AuthenticationHeaderValue authenticationHeader,
        HttpRequestMessage request,
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        tokenProvider
            .GetTokenAsync(default)
            .ReturnsForAnyArgs(authenticationHeader);
        handler
            .InvokeProtectedMethod<Task<HttpResponseMessage>>("SendAsync", request, cancellationToken)
            .ReturnsForAnyArgs(response);
        sut.InnerHandler = handler;

        using var client = new HttpClient(sut);
        await client.SendAsync(request, cancellationToken);

        _ = tokenProvider
            .Received(1)
            .GetTokenAsync(Arg.Any<CancellationToken>());
    }
}
