using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Cabazure.Client.SourceGenerator;

public class SourceGeneratorException : Exception
{
    public SourceGeneratorException(
        params Diagnostic[] diagnostics)
    {
        Diagnostics = diagnostics.ToImmutableArray();
    }

    public ImmutableArray<Diagnostic> Diagnostics { get; }
}
