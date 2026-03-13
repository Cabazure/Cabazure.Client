# Decision: BC-01 & BC-03 Runtime Binary-Compat Fixes

**Date:** 2026-03-11  
**Author:** Dallas  
**Branch:** feat/optimization-backlog  
**Commits:** 2d2c762, b2428ac  
**Status:** Approved & Executed

---

## BC-01 — Restore Binary-Compatible Constructors on Verb Attributes

### Problem
The optimization session changed all five HTTP verb attribute constructors from `(string routeTemplate)` to `(string routeTemplate, params int[] successStatusCodes)`. While source-compatible, this removed the original IL constructor signature. Binary consumers compiled against the old API (e.g., NuGet packages referencing a previous build) throw `MissingMethodException` at runtime when attempting to call the old single-arg constructor.

### Decision
Add back the original single-argument constructor to each attribute by chaining it to the params constructor:

```csharp
// Backward-compat constructor — preserves original IL signature
public GetAttribute(string routeTemplate)
    : this(routeTemplate, Array.Empty<int>()) { }
```

### Rationale
- Chain-to-params means zero logic duplication
- `Array.Empty<int>()` is allocation-free and netstandard2.0 compatible
- The params constructor applies the per-verb defaults when the array is empty, so behaviour is identical to the old single-arg constructor
- No changes to the generator, descriptors, or tests required — no generated code references the constructors

### Applied to
- `GetAttribute` — defaults: `[200]`
- `PostAttribute` — defaults: `[200, 201]`
- `PutAttribute` — defaults: `[200]`
- `DeleteAttribute` — defaults: `[200, 204]` (also repaired: params constructor was commented out, `SuccessStatusCodes` was unset)
- `PatchAttribute` — defaults: `[200, 204]`

---

## BC-03 — Expose HttpVersion on ClientRequestOptions for Opt-In HTTP/2

### Problem
The optimization session removed `message.Version = new Version(2, 0)` from `MessageRequestBuilder.Build()`. This was correct — hardcoding HTTP/2 prevents fallback to 1.1 and blocks upgrades to HTTP/3. However, it left no mechanism for consumers who genuinely need HTTP/2 to opt in.

### Decision
Add `public Version? HttpVersion { get; set; }` to `ClientRequestOptions` and apply it in `ConfigureHttpRequest()`:

```csharp
if (HttpVersion is { } version)
    request.Version = version;
```

### Rationale
- `ConfigureHttpRequest()` is the established hook where `ClientRequestOptions` stamps properties onto the outgoing `HttpRequestMessage`
- This approach requires no changes to `MessageRequestBuilder` (which only knows about the `IRequestOptions` interface, not `ClientRequestOptions`)
- `PagedRequestOptions` inherits `HttpVersion` automatically — no changes needed there
- Consumers who need HTTP/2: `options.HttpVersion = new Version(2, 0)`; everyone else gets automatic negotiation

### Verified
- ✅ Build succeeded (0 warnings, 0 errors)
- ✅ 170 tests passed (84 Runtime, 53 Integration, 33 Generator)
