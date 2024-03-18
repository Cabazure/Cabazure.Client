﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cabazure.Client.SourceGenerator.Tests;

public static class TestHelper
{
    private const string GlobalUsings = """
        // <auto-generated/>
        global using global::System;
        global using global::System.Collections.Generic;
        global using global::System.IO;
        global using global::System.Linq;
        global using global::System.Net.Http;
        global using global::System.Threading;
        global using global::System.Threading.Tasks;
        global using global::Cabazure.Client;
        """;

    public static SettingsTask Verify(params string[] sources)
    {
        var compilation = CompileSources(sources);

        var generator = new ClientEndpointGenerator();

        var results = CSharpGeneratorDriver
            .Create(generator)
            .RunGenerators(compilation)
            .GetRunResult();

        return Verifier.Verify(results);
    }

    public static SettingsTask VerifyInitialization(params string[] sources)
    {
        var compilation = CompileSources(sources);

        var generator = new ClientInitializationGenerator();

        var results = CSharpGeneratorDriver
            .Create(generator)
            .RunGenerators(compilation)
            .GetRunResult();

        return Verifier.Verify(results);
    }

    private static CSharpCompilation CompileSources(string[] sources)
    {
        var syntaxTrees = sources
            .Append(GlobalUsings)
            .Select(s => CSharpSyntaxTree.ParseText(s));

        var references = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(x => !x.IsDynamic && !string.IsNullOrWhiteSpace(x.Location))
            .Append(typeof(ClientEndpointAttribute).Assembly)
            .Select(x => MetadataReference.CreateFromFile(x.Location));

        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "Tests",
            syntaxTrees,
            references,
            new(OutputKind.DynamicallyLinkedLibrary));

        var errors = compilation
            .GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .Select(d => d.GetMessage())
            .ToArray();

        if (errors.Length > 0)
        {
            throw new ArgumentException(string.Join("\n", errors), nameof(sources));
        }

        return compilation;
    }
}
