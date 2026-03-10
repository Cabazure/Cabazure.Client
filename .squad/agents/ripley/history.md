# Project Context

- **Owner:** Ricky Kaare Engelharth
- **Project:** Cabazure.Client — a .NET library for generating strongly-typed HTTP clients via C# Source Generators
- **Stack:** C# / .NET 9, netstandard2.0 targets, IIncrementalGenerator, Roslyn (Microsoft.CodeAnalysis.CSharp 4.14), xunit v3, Verify snapshot testing, Azure.Core / Azure.Identity
- **Created:** 2026-03-10

## Learnings

- Two source projects: `Cabazure.Client` (the generator, netstandard2.0) and `Cabazure.Client.Runtime` (attributes + runtime types, netstandard2.0)
- Generators use modern `IIncrementalGenerator` API. Entry points: `ClientEndpointGenerator` and `ClientInitializationGenerator`
- Descriptors in `src/Cabazure.Client/Descriptors/` are the AST-analysis layer — changes to attributes must be reflected there
- `TypeConstants.cs` centralizes all fully-qualified attribute type names used by generators
- Test snapshots live as `*.g.verified.cs` files; `*.g.received.cs` appears when output changes and must be accepted
- `EnforceExtendedAnalyzerRules=true` — Roslyn analyzer rules are enforced strictly
- `EnablePackageValidation=true` — NuGet package structure is validated on build
- Build: `dotnet build -c Release` | Test: `dotnet test -c Release` | Single test: `dotnet test --filter "FullyQualifiedName~{TestName}"`
