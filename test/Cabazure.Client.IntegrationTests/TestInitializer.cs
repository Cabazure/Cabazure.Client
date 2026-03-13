using System.Runtime.CompilerServices;

namespace Cabazure.Client.IntegrationTests;

internal static class TestInitializer
{
    [ModuleInitializer]
    public static void Initialize()
        => FixtureFactory.Customizations.Add(new AutoFixtureCustomization());
}
