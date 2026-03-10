# Project Context

- **Owner:** Ricky Kaare Engelharth
- **Project:** Cabazure.Client — a .NET library for generating strongly-typed HTTP clients via C# Source Generators
- **Stack:** C# / .NET 9, netstandard2.0 targets, IIncrementalGenerator, Roslyn, xunit v3, Verify.XUnitV3 + Verify.SourceGenerators for snapshot testing, Atc.Test
- **Created:** 2026-03-10

## Learnings

- Three test projects: `Cabazure.Client.Tests` (generator snapshots), `Cabazure.Client.Runtime.Tests` (runtime), `Cabazure.Client.IntegrationTests` (end-to-end, AOT-compatible)
- Snapshot workflow: test runs → `*.g.received.cs` appears → inspect → rename to `*.g.verified.cs` to accept
- `TestHelper.VerifyEndpoint(...)` is the entry point for all generator snapshot tests
- Diagnostics tests live in `GeneratorDiagnosticsTests.cs` — every validation rule needs coverage here
- Single test: `dotnet test --filter "FullyQualifiedName~{TestMethodName}"`
- Integration tests are AOT-compatible — avoid reflection
- Test methods return `Task` for async verification; `[Fact]` by default, `[Theory]` for parameterized cases
