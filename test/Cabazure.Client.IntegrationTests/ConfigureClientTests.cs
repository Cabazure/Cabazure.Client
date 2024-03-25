using System.Text.Json;
using Cabazure.Client.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cabazure.Client.IntegrationTests;

public class ConfigureClientTests
{
    [Theory, AutoNSubstituteData]
    public void Should_Register_MessageRequestFactory(
        ServiceCollection services,
        [Substitute] Action<IHttpClientBuilder> builder,
        [Substitute] Action<JsonSerializerOptions> jsonOptions)
    {
        services.ConfigureClient("TestClient", builder, jsonOptions);

        services
            .Should()
            .Contain(s
                => s.Lifetime == ServiceLifetime.Singleton
                && s.ServiceType == typeof(IMessageRequestFactory)
                && s.ImplementationType == typeof(MessageRequestFactory));
    }

    [Theory, AutoNSubstituteData]
    public void Should_Register_ClientSerializer(
        ServiceCollection services,
        [Substitute] Action<IHttpClientBuilder> builder,
        [Substitute] Action<JsonSerializerOptions> jsonOptions)
    {
        services.ConfigureClient("TestClient", builder, jsonOptions);

        services
            .Should()
            .Contain(s
                => s.Lifetime == ServiceLifetime.Singleton
                && s.ServiceType == typeof(IClientSerializer)
                && s.ImplementationType == typeof(ClientSerializer));
    }

    [Theory, AutoNSubstituteData]
    public void Should_Register_JsonOptions(
        ServiceCollection services,
        [Substitute] Action<IHttpClientBuilder> builder,
        [Substitute] Action<JsonSerializerOptions> jsonOptions)
    {
        services.ConfigureClient("TestClient", builder, jsonOptions);

        services
            .Should()
            .Contain(s
                => s.Lifetime == ServiceLifetime.Singleton
                && s.ServiceType == typeof(IOptionsMonitor<>)
                && s.ImplementationType == typeof(OptionsMonitor<>));

        services
            .Should()
            .Contain(s
                => s.Lifetime == ServiceLifetime.Singleton
                && s.ServiceType == typeof(IConfigureOptions<JsonSerializerOptions>)
                && s.ImplementationInstance is ConfigureNamedOptions<JsonSerializerOptions>
                && ((ConfigureNamedOptions<JsonSerializerOptions>)s.ImplementationInstance).Name == "TestClient"
                && ((ConfigureNamedOptions<JsonSerializerOptions>)s.ImplementationInstance).Action == jsonOptions);
    }

    [Theory, AutoNSubstituteData]
    public void Should_Register_HttpClient(
        ServiceCollection services,
        [Substitute] Action<IHttpClientBuilder> builder,
        [Substitute] Action<JsonSerializerOptions> jsonOptions)
    {
        services.ConfigureClient("TestClient", builder, jsonOptions);

        services
            .Should()
            .Contain(s
                => s.Lifetime == ServiceLifetime.Singleton
                && s.ServiceType == typeof(IHttpClientFactory));

        builder
            .Received(1)
            .Invoke(Arg.Is<IHttpClientBuilder>(b => b.Name == "TestClient"));
    }
}
