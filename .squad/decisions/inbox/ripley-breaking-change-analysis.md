# Breaking Change Analysis: `feat/optimization-backlog` vs `main`

**Author:** Ripley  
**Date:** 2026-03-11  
**Branch:** `feat/optimization-backlog`  
**Scope:** `src/Cabazure.Client.Runtime/`

---

## Executive Summary

The branch introduces **one binary-breaking change** in the public attribute API surface, **four behavioral breaking changes** in the runtime, and several additive improvements that are fully backward-compatible. No source-level recompilation breaks were found — code that is recompiled against the new library builds cleanly. However, any consumer assembly compiled against the previous version of the library will fail at runtime if the runtime DLL is swapped without recompilation.

**Recommendation:** This warrants a minor version bump (1.x → 1.y) with clear upgrade notes. The binary break on attribute constructors is the only item requiring consumer attention; everything else is either additive or a behavioral improvement they will want.

---

## Breaking Change Register

### BC-01 — Attribute Constructor Signature Change (GetAttribute, PostAttribute, PutAttribute, DeleteAttribute)

**Severity:** S1 — Binary breaking for pre-compiled assemblies; source-compatible on recompile

**What changed:**

All four verb attributes had their constructor extended with a `params int[]` second parameter.

| Attribute | Old Constructor | New Constructor |
|-----------|----------------|----------------|
| `GetAttribute` | `(string routeTemplate)` | `(string routeTemplate, params int[] successStatusCodes)` |
| `PostAttribute` | `(string routeTemplate)` | `(string routeTemplate, params int[] successStatusCodes)` |
| `PutAttribute` | `(string routeTemplate)` | `(string routeTemplate, params int[] successStatusCodes)` |
| `DeleteAttribute` | `(string routeTemplate)` | `(string routeTemplate, params int[] successStatusCodes)` |

**New property added to each:** `public int[] SuccessStatusCodes { get; }`

**Old client code:**
```csharp
[Get("/customers/{id}")]
Task<EndpointResponse<Customer>> GetAsync([Path] string id, CancellationToken ct);

[Post("/customers")]
Task<EndpointResponse<Customer>> CreateAsync([Body] Customer body, CancellationToken ct);
```

**How it breaks:**

- **Source level:** Recompiling against the new library compiles cleanly. The `params` makes the second argument optional — `[Get("/customers/{id}")]` resolves to `GetAttribute(routeTemplate, params int[] = {})`. ✅
- **Binary level:** Any assembly compiled against the old library has `newobj GetAttribute::.ctor(System.String)` in its IL. The new library only exposes `GetAttribute::.ctor(System.String, System.Int32[])`. Loading the new runtime DLL without recompiling will throw `MissingMethodException` at runtime. ❌

**Affected scenario:** Any consumer who upgrades the NuGet package at runtime (e.g., in a plugin host, or via direct DLL replacement) without recompiling their client code. Standard package upgrade + rebuild is unaffected.

**Default success codes introduced:**

| Attribute | Default `SuccessStatusCodes` |
|-----------|------------------------------|
| `GetAttribute` | `[200]` |
| `PostAttribute` | `[200, 201]` |
| `PutAttribute` | `[200]` |
| `DeleteAttribute` | `[200, 204]` |

---

### BC-02 — `WithRequestOptions` No Longer Applies Timeout to `HttpClient.Timeout`

**Severity:** S2 — Behavioral change

**What changed:**

`HttpClientExtensions.WithRequestOptions(this HttpClient, IRequestOptions?)` previously mutated `httpClient.Timeout` when a timeout was specified in request options. The new version returns early without applying the timeout.

**Old behavior:**
```csharp
// Old: WithRequestOptions set httpClient.Timeout = timeout
httpClient.WithRequestOptions(options).SendAsync(request, ct);
// Result: httpClient.Timeout was mutated (thread-unsafe side-effect)
```

**New behavior:**

Timeout is applied per-request via a new `SendAsync` extension method using `CancellationTokenSource.CancelAfter`. The old `WithRequestOptions` method no longer touches `httpClient.Timeout` at all.

**Who is affected:** Any code calling `WithRequestOptions` directly and expecting it to configure the client timeout. Generated client code is unaffected — the generator was updated to call the new `SendAsync` overload. User code invoking `WithRequestOptions` manually for timeout purposes will silently stop applying timeouts.

**Why this happened:** `HttpClient` is shared across concurrent requests. Mutating `httpClient.Timeout` is not thread-safe. The new pattern isolates timeout to the individual request via `CancellationTokenSource`. This is the correct fix — but it silently changes behavior for anyone relying on the old side effect.

---

### BC-03 — HTTP/2 No Longer Hardcoded on Requests

**Severity:** S2 — Behavioral change

**What changed:**

`MessageRequestBuilder.Build()` previously set `message.Version = new Version(2, 0)` on every `HttpRequestMessage`. This line was removed.

**Old behavior:** Every outbound request used HTTP/2 explicitly.  
**New behavior:** HttpClient's default negotiation applies (HTTP/1.1 with upgrade negotiation depending on handler configuration).

**Who is affected:** Any consumer relying on HTTP/2 semantics — multiplexing, header compression, server push. In practice, most REST API scenarios won't notice, but high-throughput clients calling HTTP/2-only backends may see performance regression or connection refused errors if the server requires HTTP/2.

**Positive side:** Removes the hard block on HTTP/1.1 fallback and HTTP/3 upgrade. APIs that don't support HTTP/2 no longer fail mysteriously.

---

### BC-04 — Empty Request Body No Longer Sent

**Severity:** S2 — Behavioral change (improvement)

**What changed:**

`MessageRequestBuilder.Build()` previously always set `message.Content = new StringContent(content)` with `Content-Type: application/json`, even when `content` was empty (no `[Body]` parameter). The new code only sets `message.Content` when `content` is non-empty.

**Old behavior:**
```
GET /customers/123
Content-Type: application/json
(empty body)
```

**New behavior:**
```
GET /customers/123
(no Content-Type header, no body)
```

**Who is affected:** API backends that validated the absence of a body on GET/DELETE requests, or that had conditional logic based on presence of `Content-Type`. Most well-behaved REST servers will be unaffected or improved.

---

### BC-05 — `BearerTokenProvider` Now Implements `IDisposable`

**Severity:** S2 — Behavioral change

**What changed:**

`BearerTokenProvider` now implements `IDisposable` (disposes an internal `SemaphoreSlim`). DI containers that register it with a scoped or singleton lifetime will now call `Dispose()` when the container scope ends.

**Old behavior:** `BearerTokenProvider` held no disposable resources; DI containers did not dispose it.  
**New behavior:** DI containers will dispose the instance; after disposal, calling `GetTokenAsync` would attempt to use a disposed semaphore.

**Who is affected:** Code that registers `BearerTokenProvider` in DI and reuses it across scope boundaries (e.g., registered as singleton, injected into short-lived scopes). Also: anyone who has mocked or subclassed `BearerTokenProvider` — their mock/subclass now needs to handle `Dispose()` or they get a `NotImplementedException` if the interface is mocked at the `IDisposable` level.

---

## Additive Changes (Non-Breaking)

| Change | Notes |
|--------|-------|
| New `PatchAttribute` | Net-new attribute. Fully additive. Consumers gain PATCH verb support. |
| New `HttpClientExtensions.SendAsync` overload | New public method. Additive. |
| `BearerTokenProvider` thread-safety fix (SemaphoreSlim double-check) | Internal improvement. API surface unchanged. |
| `ConfigureAwait(false)` added throughout | Internal improvement. No observable API change. |
| `HttpRequestMessage.Options` for .NET 5+ (scopes storage) | Conditional compilation; backward-compatible via `#if`. |
| `ClientRequestOptions` and `PagedRequestOptions` | **No changes.** Identical on both branches. |

---

## Risk Assessment

| ID | Description | Severity | Consumer Action Required |
|----|-------------|----------|--------------------------|
| BC-01 | Attribute constructor signatures changed | **S1 binary** / S0 source | Rebuild against new package. No code changes needed. |
| BC-02 | `WithRequestOptions` timeout side-effect removed | S2 | Migrate to new `SendAsync(request, options, ct)` if calling directly. |
| BC-03 | HTTP/2 no longer hardcoded | S2 | Configure `HttpClient` handler for HTTP/2 explicitly if required. |
| BC-04 | Empty body no longer sent | S2 | Likely a transparent improvement. Validate against backend. |
| BC-05 | `BearerTokenProvider` is now disposable | S2 | Review DI lifetime registration. |

---

## Verdict

**Safe to ship with a minor version bump.** The attribute constructor change (BC-01) is the only item that creates a hard runtime failure, and only for consumers who replace the DLL without recompiling — the standard NuGet upgrade path requires a rebuild and is unaffected. The behavioral changes (BC-02 through BC-05) are all improvements that fix documented bugs or design flaws identified in the prior security/API audit. No changes to `ClientRequestOptions`, `PagedRequestOptions`, or the response wrapper types.

**Version recommendation:** 1.x → 1.(x+1) with a changelog entry calling out BC-01 explicitly.
