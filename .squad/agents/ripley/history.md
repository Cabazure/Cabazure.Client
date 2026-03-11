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

### TFM Upgrade Analysis (2026-03-10)
- **Current state:** src/ locked to netstandard2.0 (Roslyn constraint), test/ on net9.0, samples mixed (netstandard2.0 for client/contracts, net9.0 for TestApp)
- **Upgrade scope:** test/ and sample TestApp can move to net10.0; src/ and sample client/contracts MUST stay netstandard2.0
- **Package compatibility:** All xunit v3, Verify, Coverlet, and CodeAnalysis packages support net10 at current versions
- **BLOCKER:** `Cabazure.Test` 1.0.1 dependency in all test projects — need to verify net10 support or find alternative
- **SDK requirement:** .NET 10 SDK required for net10 targets; global.json and both CI workflows (.github/workflows/ci.yml, release.yml) must update from 9.0.x to 10.0.x
- **Risk areas:** Verify snapshot tests may detect runtime differences; new analyzer warnings under EnforceExtendedAnalyzerRules; IsAotCompatible flag in IntegrationTests may surface new AOT warnings
- **Recommendation:** Single atomic commit upgrading all test/sample TFMs + SDK + CI once blockers resolved

### Lead Security & API Design Analysis (2025-01-27)
- **Security posture:** Generally solid. Token caching in `BearerTokenProvider` is safe but could optimize for thundering-herd. `HttpRequestMessage.Properties` usage is obsolete in .NET 5+ (should migrate to `.Options`). No header validation allows potential override of `Authorization` header or invalid header injection.
- **Critical API gap:** PATCH verb missing. This is a standard REST verb (RFC 5789), and users will expect it. Straightforward addition: `PatchAttribute.cs` + TypeConstants + generator case.
- **Empty body issue:** `MessageRequestBuilder.Build` always creates `StringContent` even when body is empty, sending empty JSON on POST/PUT/DELETE with no `[Body]` parameter. Some APIs reject this. Fix: only set `message.Content` when `content` is non-empty.
- **Timeout footgun:** `ClientRequestOptions.Timeout` mutates `httpClient.Timeout`, which is not thread-safe for concurrent requests. Should use `CancellationTokenSource.CancelAfter` or `HttpRequestMessage.Options` (net5+).
- **Success status codes hardcoded:** Generator always assumes 200 OK. POST typically returns 201 Created, DELETE returns 204 No Content. No way for users to override. Should add optional `SuccessStatusCodes` param to verb attributes or broaden defaults.
- **Generated code quality:** Clean and readable. HTTP/2 is hardcoded (`message.Version = new Version(2, 0)`), which may cause issues in legacy environments. Minor: nullable annotations incomplete in generated code.
- **Breaking-change risk:** Adding new attributes is low-risk. Changing existing attribute constructors is high-risk (overload resolution). Generated class names (`internal partial class TestEndpoint`) could collide if user defines same class. Internal interfaces (`IMessageRequestFactory`, `IClientSerializer`) are part of generator contract — renaming is breaking.
- **Ship-blockers:** None. High-priority improvements: PATCH support, timeout fix, empty body handling. Defer to v2: streaming, base path configuration, custom continuation tokens.
- **Verdict:** Production-ready library with solid architecture. Address HIGH/MED findings before 1.0 release.
