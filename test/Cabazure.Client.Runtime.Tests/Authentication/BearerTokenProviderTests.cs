using Azure.Core;
using Cabazure.Client.Authentication;

namespace Cabazure.Client.Runtime.Tests.Authentication;

public class BearerTokenProviderTests
{
    [Theory, AutoNSubstituteData]
    public async Task Should_Return_Token_From_TokenCredential(
        [Frozen] TokenCredential credential,
        [Frozen] IDateTimeProvider dateTimeProvider,
        [Frozen] TokenRequestContext context,
        BearerTokenProvider sut,
        DateTimeOffset timestamp,
        AccessToken accessToken,
        CancellationToken cancellationToken)
    {
        dateTimeProvider
            .GetDateTime()
            .Returns(timestamp);

        credential
            .GetTokenAsync(default, default)
            .ReturnsForAnyArgs(accessToken);

        var response = await sut.GetTokenAsync(cancellationToken);

        response
            .Parameter
            .Should()
            .Be(accessToken.Token);
    }

    [Theory, AutoNSubstituteData]
    public async Task Should_Use_Cached_Token_From_TokenCredential(
        [Frozen] TokenCredential credential,
        [Frozen] IDateTimeProvider dateTimeProvider,
        [Frozen] TokenRequestContext context,
        BearerTokenProvider sut,
        DateTimeOffset timestamp,
        CancellationToken cancellationToken)
    {
        dateTimeProvider
            .GetDateTime()
            .Returns(timestamp);

        var accessToken = new AccessToken(
            "token",
            timestamp.AddDays(1));

        credential
            .GetTokenAsync(default, default)
            .ReturnsForAnyArgs(accessToken);

        var response1 = await sut.GetTokenAsync(cancellationToken);
        var response2 = await sut.GetTokenAsync(cancellationToken);

        response1
            .Should()
            .BeEquivalentTo(response2);

        _ = credential
            .Received(1)
            .GetTokenAsync(
                Arg.Any<TokenRequestContext>(),
                Arg.Any<CancellationToken>());
    }

    [Theory, AutoNSubstituteData]
    public async Task Should_Renew_Expired_Token(
        [Frozen] TokenCredential credential,
        [Frozen] IDateTimeProvider dateTimeProvider,
        [Frozen] TokenRequestContext context,
        BearerTokenProvider sut,
        DateTimeOffset timestamp,
        CancellationToken cancellationToken)
    {
        dateTimeProvider
            .GetDateTime()
            .Returns(timestamp);

        var accessToken = new AccessToken(
            "token",
            timestamp.AddDays(-1));

        credential
            .GetTokenAsync(default, default)
            .ReturnsForAnyArgs(accessToken);

        await sut.GetTokenAsync(cancellationToken);
        await sut.GetTokenAsync(cancellationToken);
        await sut.GetTokenAsync(cancellationToken);

        _ = credential
            .Received(3)
            .GetTokenAsync(
                Arg.Any<TokenRequestContext>(),
                Arg.Any<CancellationToken>());
    }

    [Theory, AutoNSubstituteData]
    public async Task Should_Use_Bearer_AuthorizationToken(
        [Frozen] TokenCredential credential,
        [Frozen] IDateTimeProvider dateTimeProvider,
        [Frozen] TokenRequestContext context,
        BearerTokenProvider sut,
        DateTimeOffset timestamp,
        CancellationToken cancellationToken)
    {
        dateTimeProvider
            .GetDateTime()
            .Returns(timestamp);

        var accessToken = new AccessToken(
            "token",
            timestamp.AddDays(1));

        credential
            .GetTokenAsync(default, default)
            .ReturnsForAnyArgs(accessToken);

        var response = await sut.GetTokenAsync(cancellationToken);

        response
            .Scheme
            .Should()
            .Be("Bearer");
    }
}
