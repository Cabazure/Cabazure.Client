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

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction
