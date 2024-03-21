using System.Runtime.CompilerServices;

namespace Cabazure.Client.Tests;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        VerifySourceGenerators.Initialize();
    }
}