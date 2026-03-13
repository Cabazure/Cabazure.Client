# Backward-Compatibility Fix Options — Verb Attribute SuccessStatusCodes

**Author:** Dallas  
**Branch analysed:** `feat/optimization-backlog`  
**Date:** 2026-03-11

---

## What Changed and What Actually Breaks

### Main branch constructor signature (all verb attributes)
```csharp
public GetAttribute(string routeTemplate)          // IL: .ctor(String)
```

### Feature branch constructor signature
```csharp
public GetAttribute(string routeTemplate, params int[] successStatusCodes)  // IL: .ctor(String, Int32[])
```

### The real breakage (clarifying the task description)
The task says "parameterless constructor" but that's slightly off — neither branch ever had a
parameterless constructor. What actually breaks is:

1. **Binary (IL) breaking change**: The old `(String)` constructor no longer exists in the new DLL.
   Any assembly that was compiled against the old `Cabazure.Client.Runtime` and is loaded against the new
   version will throw `MissingMethodException` at runtime.

2. **Source-level**: `[Get("path")]` **still compiles** against the feature-branch attribute — `params int[]`
   allows zero elements, so existing source code re-compiles clean. There is no source-level regression.

So the only hard problem is **binary compatibility**.

---

## Option A — Overloaded Constructors (add the old one back)

Keep the original single-arg constructor alongside the new `params` constructor via constructor chaining:

```csharp
// Backward-compat constructor — preserves the original IL signature
public GetAttribute(string routeTemplate)
    : this(routeTemplate, Array.Empty<int>()) { }

// New constructor — opt-in override
public GetAttribute(string routeTemplate, params int[] successStatusCodes)
{
    RouteTemplate = routeTemplate;
    SuccessStatusCodes = successStatusCodes.Length > 0
        ? successStatusCodes
        : [200];
}
```

### Backward compat
- **Binary ✅** — `(String)` constructor re-exists in the DLL; old compiled assemblies load fine.
- **Source ✅** — `[Get("path")]`, `[Get("path", 200)]`, `[Get("path", 200, 201)]` all compile.

### Generator/descriptor impact
**Minimal.** The existing `GetSuccessStatusCodes` logic in `EndpointMethodDescriptor` already handles both cases correctly:
- When called via the old `(string)` constructor, the Roslyn operation has no `successStatusCodes` argument
  → `statusCodesArg` is null → falls back to `defaults`. ✓
- When called via the `params` constructor with no extra args, the Roslyn operation has an empty array arg
  → `codes.Length == 0` → falls back to `defaults`. ✓
- When called with explicit codes, reads them normally. ✓

No descriptor changes required.

### Caveats
- Each attribute file gets two constructors — minor code noise.
- C# constructor chaining with `Array.Empty<int>()` is fine at the IL level (this is not an attribute-argument expression; it's inside the constructor body).

---

## Option B — Named Property with Attribute Initializer Syntax

Remove `SuccessStatusCodes` from the constructor entirely. Restore the single-arg constructor and expose
`SuccessStatusCodes` as a settable property:

```csharp
public GetAttribute(string routeTemplate)
{
    RouteTemplate = routeTemplate;
    SuccessStatusCodes = [200];   // built-in default
}

public int[] SuccessStatusCodes { get; set; }
```

Usage at the call site:
```csharp
[Get("/items", SuccessStatusCodes = new int[] { 200, 201 })]
```

### Backward compat
- **Binary ✅** — Original `(String)` constructor restored verbatim.
- **Source ✅** — All existing `[Get("path")]` usages unchanged.

### Generator/descriptor impact
**Moderate.** The current `GetSuccessStatusCodes` method reads `ConstructorArguments` via the Roslyn
operation tree (`IObjectCreationOperation.Arguments`). Property initializer-style named args in attributes
appear on `IAttributeOperation` as `NamedArguments` / initializer operations — a different path.
The descriptor would need an updated read strategy:

```csharp
// current: reads constructor args
var statusCodesArg = oco?.Arguments.FirstOrDefault(a => a.Parameter?.Name == "successStatusCodes");

// new: read from named attribute arguments
var namedArg = attributeData.NamedArguments.FirstOrDefault(a => a.Key == "SuccessStatusCodes");
```

This is straightforward but does require touching `GetSuccessStatusCodes` and potentially the
`GetAttributeValue` / `GetAttributeValues` extension helpers.

### Caveats
- **Verbose user syntax**: `new int[] { 200, 201 }` instead of `200, 201`. (Attribute named-argument
  arrays cannot use collection-expression syntax `[200, 201]` — only `new int[] { ... }` is valid.)
- A mutable setter on an otherwise immutable attribute feels slightly wrong by design, though it is
  the standard C# pattern for optional attribute arguments (`ObsoleteAttribute.IsError` etc.).
- netstandard2.0: no issue — settable properties on attributes are standard.
- Roslyn parsing change required in the descriptor layer.

---

## Option C — Keep `params`, Accept Source-Only Compat (No Change)

Verify that the current feature-branch state is already source-compatible and ship as-is,
accepting the binary breaking change as intentional for a version bump.

### Analysis
- `[Get("path")]` → compiles fine against `(string, params int[])`. ✓
- `[Get("path", 200, 201)]` → compiles fine. ✓
- `[Get]` (no args) → **never compiled on main either** — not a regression.
- Binary compat: ❌ Old compiled assemblies that hold a reference to `GetAttribute(String)` will throw
  `MissingMethodException`. This is a real breaking change for binary consumers.

### When this is acceptable
- The library is pre-1.0 / actively iterated on, and consumers are expected to re-compile on update.
- A SemVer **major version bump** is shipped alongside (signalling intentional API break).
- The team agrees "binary compat is not a concern at this stage."

### Generator/descriptor impact
None — already implemented.

### Caveats
- Requires a SemVer major bump if we care about semantic versioning.
- Forces all consumers to recompile — no silent safe upgrade path.

---

## Option D — Default Parameter Value (not recommended for attributes)

Provide a default value for the `int[]` parameter:

```csharp
public GetAttribute(string routeTemplate, int[] successStatusCodes = null)
```

Default parameter values in attributes are **not supported in C#** for arrays — the compiler rejects
`= null` for array-typed parameters in attribute constructors because attribute arguments must be
compile-time constants (and `null` for an array is technically allowed but `null` for the `params` form
is ambiguous). More importantly, this does **not** fix the binary compat issue: the IL signature is still
`(String, Int32[])` and the old `(String)` constructor is still gone.

**Verdict: Not viable.**

---

## Recommendation

### Primary: **Option A — Overloaded Constructors**

This is the right call:

1. **Full binary + source compat** with zero compromises.
2. **Zero descriptor changes** — the existing `GetSuccessStatusCodes` logic already handles the
   empty-params-array case and the no-argument case identically (both fall back to defaults).
3. **Best user syntax** — `[Get("path", 200, 201)]` is idiomatic; no `new int[]` wrapper needed.
4. **Consistent with the netstandard2.0 constraint** — constructor chaining with `Array.Empty<int>()`
   works fine; no new framework APIs required.
5. **Change scope is tiny** — five attribute files, two constructors each, ~3 lines per file added.

### Secondary consideration: Option B
If the team wants to enforce that `SuccessStatusCodes` is always explicit and clearly named at the
call site, Option B is valid but carries the verbose `new int[] { ... }` syntax cost and a moderate
descriptor update. Not recommended unless there is a strong API-readability argument for it.

---

## Implementation Sketch for Option A

For each of the five attribute files (`GetAttribute.cs`, `PostAttribute.cs`, `PutAttribute.cs`,
`DeleteAttribute.cs`, `PatchAttribute.cs`), add the backward-compat constructor **above** the params
constructor:

```csharp
// GetAttribute.cs
public GetAttribute(string routeTemplate)
    : this(routeTemplate, Array.Empty<int>()) { }

public GetAttribute(string routeTemplate, params int[] successStatusCodes)
{
    RouteTemplate = routeTemplate;
    SuccessStatusCodes = successStatusCodes.Length > 0
        ? successStatusCodes
        : [200];
}
```

The defaults array per verb is unchanged:
- GET: `[200]`
- POST: `[200, 201]`
- PUT: `[200]`
- DELETE: `[200, 204]`
- PATCH: `[200, 204]`

No changes required to the generator, descriptors, `TypeConstants.cs`, or any test snapshots.
Existing tests remain green without modification.
