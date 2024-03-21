namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static void AddTestStubClient(
        this IServiceCollection services)
        => services.ConfigureClient(
            "TestStub",
            c => c.ConfigureHttpClient(c => c.BaseAddress = new("http://test.org")),
            j => { });
}
