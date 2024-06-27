using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace Cabazure.Client.SourceGenerator;

[ExcludeFromCodeCoverage]
public class SourceGeneratorException : Exception
{
    public SourceGeneratorException(
        params Diagnostic[] diagnostics)
    {
        Diagnostics = diagnostics.ToImmutableArray();
    }

    public ImmutableArray<Diagnostic> Diagnostics { get; }
}
