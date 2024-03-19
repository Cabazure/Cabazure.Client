using System.Text.Json;
using Cabazure.Client;

namespace Microsoft.Extensions.DependencyInjection;

public static partial class ServiceCollectionExtensions
{
    public static void AddTestStubClient(
        this IServiceCollection services)
        => services.RegisterTestStubClient(
            c => c.ConfigureHttpClient(c => c.BaseAddress = new("http://test.org")),
            j => { });

    [ClientInitialization("TestStub")]
    static partial void RegisterTestStubClient(
        this IServiceCollection services,
        Action<IHttpClientBuilder> builder,
        Action<JsonSerializerOptions> jsonOptions);
}
