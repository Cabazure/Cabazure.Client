using Atc.Test.Customizations;
using AutoFixture;
using Cabazure.Client.Builder;

namespace Cabazure.Client.IntegrationTests;

[AutoRegister]
public class AutoFixtureCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        fixture
            .Customize<IHttpClientFactory>(c => c
            .FromFactory((IFixture f) => CreateHttpClientFactory(f)));

        fixture
            .Customize<HttpClient>(c => c
            .FromFactory((IFixture f) => CreateHttpClient(f)));

        fixture
            .Customize<IMessageRequestFactory>(c => c
            .FromFactory((IFixture f) => CreateMessageRequestFactory(f)));

        fixture
            .Customize<IMessageRequestBuilder>(c => c
            .FromFactory((IFixture f) => CreateMessageRequestBuilder(f)));

        fixture
            .Customize<IMessageResponseBuilder>(c => c
            .FromFactory((IFixture f) => CreateMessageResponseBuilder(f)));
    }

    private static IHttpClientFactory CreateHttpClientFactory(IFixture fixture)
    {
        var factory = Substitute.For<IHttpClientFactory>();
        factory
            .CreateClient(Arg.Any<string>())
            .Returns(c => fixture.Create<HttpClient>());

        return factory;
    }

    private static HttpClient CreateHttpClient(IFixture fixture)
        => Substitute.For<HttpClient>();

    private static IMessageRequestFactory CreateMessageRequestFactory(IFixture fixture)
    {
        var factory = Substitute.For<IMessageRequestFactory>();
        factory
            .FromTemplate(Arg.Any<string>(), Arg.Any<string>())
            .Returns(c => fixture.Create<IMessageRequestBuilder>());
        factory
            .FromResponse(Arg.Any<string>(), Arg.Any<HttpResponseMessage>())
            .Returns(c => fixture.Create<IMessageResponseBuilder>());

        return factory;
    }

    private static IMessageRequestBuilder CreateMessageRequestBuilder(IFixture fixture)
    {
        var builder = Substitute.For<IMessageRequestBuilder>();
        builder
            .WithBody<object>(default)
            .ReturnsForAnyArgs(builder);
        builder
            .WithHeader(default, default)
            .ReturnsForAnyArgs(builder);
        builder
            .WithPathParameter(default, default)
            .ReturnsForAnyArgs(builder);
        builder
            .WithQueryParameter(default, default)
            .ReturnsForAnyArgs(builder);
        builder
            .WithRequestOptions(default)
            .ReturnsForAnyArgs(builder);
        builder
            .Build(default)
            .ReturnsForAnyArgs(c => fixture.Create<HttpRequestMessage>());

        return builder;
    }

    private static IMessageResponseBuilder CreateMessageResponseBuilder(IFixture fixture)
    {
        var builder = Substitute.For<IMessageResponseBuilder>();
        builder
            .AddSuccessResponse(default)
            .Returns(builder);
        builder
            .AddSuccessResponse<object>(default)
            .ReturnsForAnyArgs(builder);
        builder
            .AddErrorResponse(default)
            .ReturnsForAnyArgs(builder);
        builder
            .AddErrorResponse<object>(default)
            .ReturnsForAnyArgs(builder);

        return builder;
    }
}
