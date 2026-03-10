# Work Routing

How to decide who handles what.

## Routing Table

| Work Type | Route To | Examples |
|-----------|----------|----------|
| Source generator implementation | Dallas | New attributes, generator emit logic, descriptor changes, TypeConstants |
| Runtime library | Dallas | Response types, builder pipeline, auth handler, options interfaces |
| Roslyn / analyzer work | Dallas | Diagnostics, syntax analysis, semantic model extensions |
| Architecture & API design | Ripley | Generator API shape, attribute contracts, breaking-change evaluation |
| Code review | Ripley | PR review, quality gates, generated code shape review |
| Scope & priorities | Ripley | What ships next, trade-offs, decisions |
| Generator snapshot tests | Hudson | New *.g.verified.cs snapshots, TestHelper usage |
| Runtime unit tests | Hudson | Builder, auth, response type tests |
| Integration tests | Hudson | End-to-end generator + runtime, AOT compatibility |
| Diagnostics tests | Hudson | GeneratorDiagnosticsTests.cs — every new diagnostic needs coverage |
| README & docs | Bishop | Public-facing documentation, how-to guides |
| Code samples | Bishop | samples/AzureRest/, inline examples |
| Release notes | Bishop | Changelog, NuGet release descriptions |
| XML doc comments | Bishop | Public API surface in Cabazure.Client.Runtime |
| Session logging | Scribe | Automatic — never needs routing |
| Work queue / backlog | Ralph | Monitoring, issue triage, keep-alive |

## Issue Routing

| Label | Action | Who |
|-------|--------|-----|
| `squad` | Triage: analyze issue, assign `squad:{member}` label | Ripley |
| `squad:ripley` | Architecture, design, code review issues | Ripley |
| `squad:dallas` | Generator/runtime implementation issues | Dallas |
| `squad:hudson` | Test failures, coverage gaps, quality issues | Hudson |
| `squad:bishop` | Documentation, sample, release note issues | Bishop |

## Rules

1. **Eager by default** — spawn all agents who could usefully start work, including anticipatory downstream work.
2. **Scribe always runs** after substantial work, always as `mode: "background"`. Never blocks.
3. **Quick facts → coordinator answers directly.** Don't spawn an agent for "what port does the server run on?"
4. **When two agents could handle it**, pick the one whose domain is the primary concern.
5. **"Team, ..." → fan-out.** Spawn all relevant agents in parallel as `mode: "background"`.
6. **New generator feature** → spawn Dallas (implement) + Hudson (snapshot tests) simultaneously.
7. **Issue-labeled work** — when a `squad:{member}` label is applied to an issue, route to that member. Ripley handles all `squad` (base label) triage.

