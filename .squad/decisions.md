# Squad Decisions

## Active Decisions

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
