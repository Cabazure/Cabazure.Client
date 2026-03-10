# Project Context

- **Project:** Cabazure.Client
- **Created:** 2026-03-10

## Core Context

Agent Scribe initialized and ready for work.

## Recent Updates

📌 Team initialized on 2026-03-10  
📌 Dallas completed TFM upgrade (net9.0 → net10.0): 115 tests passed  
📌 Consolidated squad logs and decisions on 2026-03-10T18-20-00Z  

## Learnings

- Library projects must remain on netstandard2.0 due to Roslyn SourceGenerator hosting constraints
- When upgrading to future .NET versions, follow pattern: test/sample projects upgrade, library projects stay on netstandard2.0
- SDK version in global.json must align with CI workflow dotnet-version specifications
