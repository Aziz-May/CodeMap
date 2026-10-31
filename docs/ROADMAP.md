# CodeMap roadmap

## Vision

CodeMap should answer one question well:

> **"If I change this piece of code, what else could be affected, and why?"**

It answers it by analyzing code **safely**: CodeMap never builds or runs what it analyzes, so it can be used on untrusted repositories, in CI and on pull requests.

## Where CodeMap stands among existing tools

A semantic dependency graph for C# is **not** new. Several free tools already build one with Roslyn, and some expose it to AI agents:

| Tool | What it already offers |
|---|---|
| [Slnmap](https://www.nuget.org/packages/Slnmap) | Roslyn graph → SQLite → MCP server, impact analysis, cycle detection, HTML graph, watch mode |
| [CSharpCodeAnalyst](https://github.com/ATrefzer/CSharpCodeAnalyst) | Dependency graph, cycle groups, AI advisor (needs MSBuild) |
| [SharpLensMcp](https://nuget.org/packages/sharplensmcp), RoslynMcp, MCPsharp | Roslyn MCP servers: find references, callers, change impact |
| [CSharpDepsGraph](https://www.nuget.org/packages/CSharpDepsGraph.Cli) | Graph export to JSON/DGML (needs a build) |
| [NDepend](https://www.ndepend.com/features/code-diff) | Commercial: dependency graph and matrix, code rules, baseline diff |
| NetArchTest / ArchUnitNET | Architecture rules as unit tests |
| Rider / ReSharper / Visual Studio | Find usages, call hierarchy, dependency diagrams |

So a graph, a visualization, a CLI or an MCP server are **not** reasons for CodeMap to exist on their own. These are the directions where it can still add real value, all of them still **hypotheses to verify**:

1. **Safe *and* accurate analysis.** No build and no MSBuild evaluation, yet close to build-level accuracy, by reading project files as plain XML and loading package references as metadata only.
2. **Trustworthy impact analysis.** Every result explains *why* something is affected (the dependency path) and states *what the tool cannot see* (reflection, DI wiring, unresolved packages).
3. **Impact + architecture rules + diff on a pull request**, as a free, CI-friendly report.

## Working principles

- **TDD.** Red, then green, then refactor. Merge only after the refactor step.
- **One step = one branch = one PR.** `main` is always green.
- **Build today's step simply, but never in a way that blocks tomorrow.** Spend the extra care only on decisions that are expensive to change later (contracts, IDs, data formats).
- **Prove value in the CLI first.** A backend and a UI come only after the core capability is shown to be useful.
- **Measure, don't claim.** Accuracy is measured against a reference, and differentiation is checked against real competing tools.
- **Track known gaps as GitHub Issues**, each with a code example that will become its test.

---

## Milestone overview

| # | Milestone | Status |
|---|---|---|
| 1 | Analysis engine + CLI (C#) | 🟡 in progress (step 8) |
| — | Research checkpoint: evaluate competing tools | ⬜ |
| 2 | A graph you can trust | ⬜ |
| 3 | Impact and diff, in the CLI and on pull requests | ⬜ |
| 4 | Decide on hosting: backend and UI | ⬜ |
| 5 | More languages | ⬜ |
| 6 | Scale | ⬜ |

---

## Milestone 1: Analysis engine + CLI

**Goal:** `codemap analyze <path>` turns a C# codebase into a correct, stable, **versioned** JSON map.

| Step | What | Status |
|---|---|---|
| 1 | `ILanguageAnalyzer` contract and class/method extraction | ✅ |
| 2 | Shared test fixture (`ShopAnalysisFixture`) | ✅ |
| 3 | Every symbol kind: interface, struct, record, enum, constructor, property, field | ✅ |
| 4 | `Calls` relations (internal, external skipped, unresolved) | ✅ |
| 5 | `Inherits`, `Implements`, `Instantiates`, `Injects` | ✅ |
| 6 | CLI: `analyze` command, JSON output, exit codes | ✅ |
| 7 | File discovery: skips build folders and generated files, ignores symlinks, returns sorted output | ✅ |
| 8 | Hardening the contract and the IDs (see below) | 🟡 next |

### Step 8: hardening

These steps fix the output contract and the IDs before anything is built on top of them.

| Step | What | Why now |
|---|---|---|
| 8.0 | Test helper that analyzes inline code snippets | Every step below needs isolated, readable tests |
| 8a | `filePath` always uses `/` ✅ | Before this, the same repo gave different JSON on Windows and Linux |
| 8b | `schemaVersion` and analyzer name/version in `AnalysisResult` | Backward compatibility can't be checked without them, and adding them later is costly |
| 8c | #5 Partial classes produce duplicate symbol IDs | Duplicate IDs break any storage that uses them as keys |
| 8d | Generic and extension method IDs don't match their declaration (`OriginalDefinition`, `ReducedFrom`) | These relations point at nothing |
| 8e | Relations inside field initializers are dropped | Silent data loss |
| 8f | An interface extending an interface is recorded as `Implements` | The relation has the wrong kind |

**Done when:** steps 8.0–8f are merged and the CLI runs on a real open-source C# project with no duplicate IDs and no crashes.

---

## Research checkpoint: evaluate competing tools

This takes about a day and comes **before** milestone 2. Run Slnmap, SharpLensMcp, CSharpCodeAnalyst and NetArchTest on **the same real open-source repository** and record:

- what each one finds for "who depends on X?"
- what each one requires (a build, MSBuild, the .NET SDK version)
- where each one is wrong or silent
- how each one explains its results, if it does

**Output:** a short written comparison that confirms, changes or drops each hypothesis listed in the vision.

---

## Milestone 2: A graph you can trust

**Goal:** the dependency graph is complete enough that "nothing is affected" can be believed.

- **The relation kinds impact analysis needs:**
  - `Uses`: type usage in fields, parameters, return types and generic arguments
  - `Reads` / `Writes`: property and field access
  - `Overrides`
  - Member-level implementations: which method implements which interface method
- **A containment hierarchy:** project → namespace → type → member. Architecture views and rules depend on it.
- **Package references without a build:** read `PackageReference`s from `.csproj` files as plain XML, with no MSBuild evaluation, and load the packages' reference assemblies as metadata only.
- **Multi-project solutions:** one compilation per project, with the references between them.
- **Missing symbols:** #6 top-level statements, #7 local functions, #8 events/indexers/operators/delegates/destructors, #9 positional record properties.
- **Smaller gaps:**
  - method groups and delegates
  - primary constructors and property injection treated as `Injects`
  - unresolved base types always being recorded as `Inherits`
- **Accuracy measurement:** on 2–3 real repositories, compare CodeMap's "who depends on X?" with Roslyn's `SymbolFinder.FindReferencesAsync` run on a built copy. Report precision and recall, and track them over time.

**Done when:** precision and recall are measured on real repositories and the share of `Unresolved` relations is low enough to rely on.

## Milestone 3: Impact and diff

**Goal:** the smallest useful version of the core capability, without a web app.

- `codemap impact <symbol>`: everything that directly or transitively depends on a symbol, with **the path that explains each result** and **the known blind spots**.
- `codemap diff <before> <after>`: symbols and relations that were added or removed, and changes recognized as changes. A signature change currently produces a new ID, so it needs to be matched to the old one.
- **Architecture rules:** simple "layer A must not depend on layer B" rules, checked by the CLI.
- **Pull request report:** a CI step that posts impact, new dependencies and rule violations as a PR comment.

## Milestone 4: Decide on hosting

The original plan put a backend (PostgreSQL, API, background worker) and a web UI here. Whether to build them, and in what form, gets decided **after** milestone 3, based on whether people use the CLI and PR report.

If we build them, these parts of the design still apply:

- analysis runs in a background worker that uses the same `CodeMap.Analysis` library, never inside an API request
- uploads are treated as untrusted: size limits, protection against ZIP path traversal, no symlinks, no code execution
- symbols and relations are inserted in batches
- the UI shows the type level by default, groups nodes by namespace or project, and lets you drill down into members

## Milestone 5: More languages

Each language gets its own analyzer behind `ILanguageAnalyzer` and produces the same JSON contract. Symbol IDs get a language prefix before the second language is added, so IDs from different languages can't collide.

| Language | Engine | Notes |
|---|---|---|
| TypeScript / JavaScript | TypeScript Compiler API | Runs in Node.js as a separate process that prints JSON |
| Python | Pyright | Dynamic typing leads to more `Unresolved` relations, and may need an `Inferred` confidence level |
| Others | tree-sitter | Syntax only, used as a fallback |

## Milestone 6: Scale

- Parallel analysis of syntax trees
- Incremental analysis that re-analyzes only the files that changed
- Streaming results instead of holding everything in memory
- CI on Linux and Windows for every pull request
