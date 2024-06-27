using Azure.Core;
using Cabazure.Client.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Cabazure.Client.Runtime.Tests.Builder;

public class HttpClientBuilderExtensionsTests
{
    [Theory, AutoNSubstituteData]
    public void SetBaseAddress_Should_Set_BaseAddress_On_HttpClient(
    ServiceCollection services,
    IHttpClientBuilder builder,
    string name,
    Uri baseAddress)
    {
        builder.Name.Returns(name);
        builder.Services.Returns(services);

        builder.SetBaseAddress(baseAddress);

        services.AddHttpClient(name);
        var httpClient = services
            .BuildServiceProvider()
            .GetRequiredService<IHttpClientFactory>()
            .CreateClient(name);
        httpClient.BaseAddress
            .Should()
            .Be(baseAddress);
    }

    [Theory, AutoNSubstituteData]
    public void AddAuthentication_With_TokenContext_Should_Configure_AzureAuthenticationHandler(
        ServiceCollection services,
        IHttpClientBuilder builder,
        string name,
        TokenRequestContext context,
        TokenCredential credential)
    {
        builder.Name.Returns(name);
        builder.Services.Returns(services);

        builder.AddAuthentication(
            context, 
            credential);

        services.AddHttpClient(name);
        var handlers = services
            .BuildServiceProvider()
            .GetRequiredService<IHttpMessageHandlerFactory>()
            .CreateHandler(name)
            .GetHandlerChain();
        handlers
            .Should()
            .ContainItemsAssignableTo<AzureAuthenticationHandler>();
    }

    [Theory, AutoNSubstituteData]
    public void AddAuthentication_With_Scope_Should_Configure_AzureAuthenticationHandler(
        ServiceCollection services,
        IHttpClientBuilder builder,
        string name,
        string scope,
        TokenCredential credential)
    {
        builder.Name.Returns(name);
        builder.Services.Returns(services);

        builder.AddAuthentication(
            scope,
            credential);

        services.AddHttpClient(name);
        var handlers = services
            .BuildServiceProvider()
            .GetRequiredService<IHttpMessageHandlerFactory>()
            .CreateHandler(name)
            .GetHandlerChain();
        handlers
            .Should()
            .ContainItemsAssignableTo<AzureAuthenticationHandler>();
    }
}
