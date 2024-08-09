using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Cabazure.Client.SourceGenerator.EmbeddedSource;

[Generator]
public class EmbeddedSourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
        => context.RegisterForPostInitialization(AddEmbeddedSources);

    public void Execute(GeneratorExecutionContext context) { }

    private static void AddEmbeddedSources(GeneratorPostInitializationContext context)
    {
        var type = typeof(EmbeddedSourceGenerator);
        var prefix = type.Namespace + ".";
        var assembly = type.Assembly;
        foreach (var sourceFile in assembly.GetManifestResourceNames())
        {
            if (sourceFile.StartsWith(prefix))
            {
                var hintName = sourceFile.Substring(prefix.Length);
                var stream = assembly.GetManifestResourceStream(sourceFile);
                context.AddSource(
                    hintName, 
                    SourceText.From(
                        stream, 
                        encoding: System.Text.Encoding.UTF8,
                        checksumAlgorithm: SourceHashAlgorithm.Sha1,
                        throwIfBinaryDetected: true,
                        canBeEmbedded: true));
            }
        }
    }
}
