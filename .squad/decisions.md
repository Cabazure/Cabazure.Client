# Squad Decisions

## Active Decisions

### Binary Compatibility & HTTP/2 Handling (2026-03-11)

**Status:** Approved & Executed  
**Author:** Dallas  
**Branch:** `feat/optimization-backlog`  
**Commits:** 2d2c762, b2428ac, 45ef2a0, d101310, a8bdd2e  
**Date:** 2026-03-11

**Decision Cluster:** Address binary compatibility breaks (BC-01, BC-03) introduced in optimization-backlog session:

#### BC-01: Restore Binary-Compatible Constructors on Verb Attributes
**What:** Add back the original single-argument constructor to all five HTTP verb attributes (Get, Post, Put, Delete, Patch) by chaining to the params constructor.

```csharp
public GetAttribute(string routeTemplate)
    : this(routeTemplate, Array.Empty<int>()) { }
```

**Why:** The optimization session changed constructors from `(string routeTemplate)` to `(string routeTemplate, params int[] successStatusCodes)`. While source-compatible, this breaks binary consumers (NuGet packages) compiled against the old API. Overloaded constructors restore full binary & source compatibility.

**How:** Constructor chaining with `Array.Empty<int>()` (allocation-free, netstandard2.0 compatible). Params constructor applies per-verb defaults when array is empty.

**Applied to:**
- `GetAttribute` — defaults: `[200]`
- `PostAttribute` — defaults: `[200, 201]`
- `PutAttribute` — defaults: `[200]`
- `DeleteAttribute` — defaults: `[200, 204]`
- `PatchAttribute` — defaults: `[200, 204]`

**Impact:** Full backward compatibility restored; no generator/descriptor/test changes needed.

#### BC-03: HTTP/2 Handling — Per-Request Version via ClientRequestOptions
**What:** Remove hardcoded `message.Version = new Version(2, 0)` from `MessageRequestBuilder.Build()` and add `public Version? HttpVersion { get; set; }` to `ClientRequestOptions`, applied in `ConfigureHttpRequest()`.

**Why:** Hardcoding HTTP/2 prevents fallback to HTTP/1.1 and blocks HTTP/3 upgrade. However, HTTP/2 should still be the library default for performance. Three approaches explored:

1. **Restore unconditionally** — Simple but breaks HTTP/1.1-only servers on older runtimes.
2. **Platform guard** — `#if NET5_0_OR_GREATER` in `MessageRequestBuilder` — still too aggressive, sets per-request when better done at client setup.
3. **HttpClient.DefaultRequestVersion (chosen)** — Set once at DI time in generated `ConfigureHttpClient` delegate, uses `HttpVersionPolicy.RequestVersionOrLower` (prefers HTTP/2, gracefully falls back). Opt-out via `ClientRequestOptions.HttpVersion` per-request override.

**How:** 
- Generator emits `client.DefaultRequestVersion = new Version(2, 0)` in `AddCabazureClient<TOptions>`, guarded by `#if NET5_0_OR_GREATER`.
- Consumers needing HTTP/1.1 or HTTP/3 override via `options.HttpVersion = new Version(1, 1)`.
- `PagedRequestOptions` inherits the opt-in property automatically.

**Key Directive:** `HttpClient.DefaultRequestVersion` only affects convenience methods (`GetAsync` etc.). When using `SendAsync(HttpRequestMessage)` with manually constructed messages, `HttpRequestMessage.Version` is authoritative. That's why the library sets it on the request inside `ConfigureHttpRequest()` when `ClientRequestOptions.HttpVersion` is specified.

**Impact:** All requests on .NET 5+ prefer HTTP/2 with graceful fallback; netstandard2.0 consumers unaffected; full opt-in override capability.

**Verified:**
- ✅ Build succeeded (0 warnings, 0 errors)
- ✅ 170 tests passed (84 Runtime, 53 Integration, 33 Generator)

---

### TFM Upgrade to .NET 10 (2026-03-10)

**Status:** Approved & Executed  
**Author:** Dallas  
**Commit:** `6fa7b90` — "build: upgrade test and sample projects to net10.0"

**Decision:** Upgrade test and sample projects from net9.0 → net10.0 while preserving netstandard2.0 targets for all library projects.

**Rationale:**
- Test/sample projects benefit from latest runtime features, diagnostics, and performance
- Library projects remain on netstandard2.0 due to Roslyn SourceGenerator constraints
- Maximizes compatibility across .NET Framework, Core, and modern .NET versions

**Constraints:**
| Project | TFM | Reason |
|---------|-----|--------|
| `src/Cabazure.Client/` | netstandard2.0 | Roslyn SourceGenerator constraint |
| `src/Cabazure.Client.Runtime/` | netstandard2.0 | Runtime for generated code |
| `samples/AzureRest/AzureRest.Client/` | netstandard2.0 | Cross-platform compatibility |
| `samples/AzureRest/AzureRest.Contracts/` | netstandard2.0 | Shared contracts |

**Verified:** ✅ Build succeeded | ✅ 115 tests passed | ✅ No new analyzer warnings

**Future Pattern:** When upgrading to .NET 11+, follow same approach — test/sample projects upgrade, library projects remain on netstandard2.0.

---

### Test Suite Performance Benchmarking (2026-03-10)

**Status:** Blocked — Requires Action  
**Reported by:** Hudson  

**Issue:** Source generator requires Roslyn 4.14.0; available .NET SDK (9.0.100-rc.2) ships with Roslyn 4.12.0. Missing .NET SDK 9.0.0 (final) causes build failures on both branches.

**Required Action:** Install .NET SDK 9.0.0 (final release) to enable benchmarking.

**Deferred Decision:**
- Should we install .NET SDK 9.0.0?
- Or relax global.json to accept RC SDK?
- Or defer benchmarking until compatible SDK is available?

### Optimization Backlog Session (2026-03-11)

**Status:** Approved & Executed  
**Team:** Ripley (analyst), Hudson (tester), Dallas (developer), Coordinator (CI)  
**Commits:** 9 (800890c, e993bc7, 255dbf6, db09d36, 9cb4eec, 6a21bcd, a6c753a, 2aeaeb8, 6181a8f)

**Decision Cluster:** Address HIGH and MED findings from optimization audit:

#### 1. Thread-Safe Token Cache (800890c)
**What:** Implement SemaphoreSlim double-check locking in BearerTokenProvider to prevent concurrent token refresh storms.  
**Why:** Multiple threads calling GetTokenAsync simultaneously near token expiry all trigger Azure credential fetch — unnecessary load and p99 latency spike.  
**How:** Fast path checks cache without lock; slow path acquires semaphore, double-checks, refreshes if needed.  
**Impact:** Eliminates race condition; improves latency under high concurrency.

#### 2. Builder HTTP Semantics (e993bc7)
**What:** 
- Conditionally allocate StringContent only for non-empty bodies
- Remove HTTP/2 hardcode, allow automatic negotiation
- Replace HttpClient.Timeout mutation with CancellationTokenSource wrapper pattern

**Why:** 
- GET/DELETE should not send body or Content-Type header
- Hardcoded HTTP/2 prevents fallback to 1.1 and prevents 3.0 upgrade
- HttpClient is shared; mutating Timeout is thread-unsafe for concurrent requests

**How:** New `SendAsync` extension on `HttpClient` taking `IRequestOptions?`; timeout via `CancellationTokenSource.CancelAfter()`.  
**Impact:** Better HTTP semantics, thread-safe per-request timeout, no breaking changes.

#### 3. Generator Enhancements (255dbf6)
**What:**
- Add PATCH HTTP verb support (PatchAttribute, generator emit)
- Optimize incremental generator predicate to filter InterfaceDeclarationSyntax early
- Extract duplicate naming logic into EndpointNaming helper
- Add ECL008 diagnostic: prevent nullable body parameters

**Why:**
- PATCH is standard RFC 5789 verb; users expect it for partial updates
- Filtering early in predicate reduces semantic analysis overhead
- Duplicate naming logic in two descriptors creates maintenance burden
- Nullable body parameters compile but fail at runtime — catch at compile-time instead

**Impact:** API completeness, generator performance, better error messages.

#### 4. Success Status Code Defaults (6a21bcd)
**What:** Generator emits multiple `.AddSuccessResponse` calls per HTTP method:
- GET: 200
- POST: 200, 201
- PUT: 200
- DELETE: 200, 204
- PATCH: 200, 204

**Why:** Different HTTP methods have different standard success codes; hardcoding 200 for all breaks common patterns (POST returns 201 Created, DELETE returns 204 No Content).  
**Impact:** Generated code matches HTTP semantics; users can override via optional attribute parameter in future.

#### 5. Test Suite Expansion (a6c753a)
**What:** Add 49 new tests covering:
- EndpointResponse<T> and PagedResponse behavior & status helpers
- All 9 runtime attributes (validation of targets, initialization)
- Options classes (ClientRequestOptions, PagedRequestOptions)
- Multiple path/query/header parameters
- URL encoding edge cases
- Nullable parameter variations
- Complex body types

**Why:** Ripley and Hudson identified 10 HIGH/MED coverage gaps; none of these areas had unit tests.  
**Impact:** 164 tests passing; increased confidence in runtime library.

#### 6. CI AOT Validation (2aeaeb8)
**What:** Add GitHub Actions job to validate AOT compatibility:
- Runs `dotnet publish -c Release /p:PublishAot=true` on integration tests
- Verifies DynamicallyAccessedMembers attributes in generated code
- Checks for AOT warnings

**Why:** Project marked `<IsAotCompatible>true</IsAotCompatible>` but no CI validation; AOT warnings only appear during publish.  
**Impact:** Prevents AOT regressions; customers using AOT can ship confidently.

#### 7. User Directive: Extension Method Preference (6181a8f)
**What:** User preference documented — prefer extension methods over static helper classes for utility logic.  
**How:** EndpointNaming refactored from static class to extension methods on InterfaceDeclarationSyntax.  
**Why:** User request; aligns with fluent API design patterns.  
**Impact:** Cleaner API surface, reduced parameter passing.

---

### Deferred to v2.0 Roadmap

Based on optimization analysis (Ripley), the following improvements are valuable but non-blocking for 1.0:

| Feature | Why Defer |
|---------|-----------|
| Streaming request/response | Complex; not needed for most REST APIs; can workaround manually |
| Base path per-endpoint | Edge case; multi-tenancy solvable with multiple clients |
| Custom continuation token extraction | Many paging conventions; document current convention; users can subclass |
| Header validation & reserved header protection | Important but lower priority than HTTP semantics fixes |

---

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction
