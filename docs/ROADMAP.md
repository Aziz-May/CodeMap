# CodeMap roadmap

## Vision

CodeMap should become a tool that can map **any** real codebase, large or small and in several languages, and show it as an interactive dependency map that people actually use to understand, review and refactor code.

We get there one milestone at a time:

1. Make the analysis correct.
2. Make it usable.
3. Make it scale.

## Working principles

- **TDD.** Red, then green, then refactor. Merge only after the refactor step.
- **One step = one branch = one PR.** `main` is always green.
- **Build today's step simply, but never in a way that blocks tomorrow.** Spend the extra care only on decisions that are expensive to change later (contracts, IDs, data formats).
- **Track known gaps as GitHub Issues**, each with a fixture example that will become its test.

---

## Milestone overview

| # | Milestone | Status |
|---|---|---|
| 1 | Analysis engine + CLI (C#) | 🟡 in progress |
| 2 | Backend: database, API, background worker | ⬜ planned |
| 3 | Frontend: upload and interactive map | ⬜ planned |
| 4 | More languages: TypeScript, Python | ⬜ planned |
| 5 | Scale and hardening | ⬜ planned |

---

## Milestone 1: Analysis engine + CLI

**Goal:** `codemap analyze <path>` turns a C# codebase into a correct, stable JSON map.

| Step | What | Status |
|---|---|---|
| 1 | `ILanguageAnalyzer` contract and class/method extraction | ✅ |
| 2 | Shared test fixture (`ShopAnalysisFixture`) | ✅ |
| 3 | Every symbol kind: interface, struct, record, enum, constructor, property, field | ✅ |
| 4 | `Calls` relations (internal, external skipped, unresolved) | ✅ |
| 5a | `Inherits` / `Implements` | ✅ |
| 5b | `Instantiates` (`new X()` and `new()`) | ✅ |
| 5c | `Injects` (constructor parameters) | ✅ |
| 6 | CLI: `analyze` command, JSON output, exit codes | ✅ |
| 7 | File discovery: skip `bin/`, `obj/`, `.git/`, `node_modules/`, generated files, symlinks; sorted output | ⬜ next |
| 8 | Edge-case hardening (the issues below) | ⬜ |

### Step 7: File discovery (next)

- A `SourceFileDiscovery` class in `CodeMap.Analysis`, so the CLI and the backend can share it.
- Walk the folder tree **without ever entering** ignored folders. This matters for speed: `node_modules/` and `.git/` can hold over 100,000 files.
- Skip symbolic links. This protects against loops and against uploads that point outside their own folder.
- Return the files sorted, so the same repo always produces the same output.
- Replace `Directory.GetFiles` in `AnalyzeCommand`.

### Known gaps

These are the gaps tracked for step 8. ⭐ = high value, meaning it turns the demo into a real tool.

**Bugs: wrong or silently missing output**

- [ ] #5 Partial classes produce duplicate symbol IDs
- [ ] Generic methods and types produce IDs that don't match their declaration (fix: `OriginalDefinition`)
- [ ] Extension method calls may not match the declared method ID (fix: `ReducedFrom`)
- [ ] Relations inside field initializers are dropped (`GetDeclaredSymbol` returns null for field declarations)
- [ ] An interface that extends another interface is recorded as `Implements` instead of `Inherits`
- [ ] `filePath` uses the OS path separator (`\` on Windows), so the same repo gives different JSON on Windows and Linux. It should always use `/`.

**Missing symbols**

- [ ] #6 Top-level statements: no `Program` symbol
- [ ] #7 Local functions
- [ ] #8 Events, indexers, operators, delegates, destructors
- [ ] #9 Positional record parameters don't produce properties

**Missing or limited relations**

- [ ] ⭐ Type usage (field, parameter and return types) as a dependency: a new `Uses` kind
- [ ] ⭐ Calls through interfaces don't link to their implementations (via `Implements` and DI registrations such as `AddScoped<IPaymentGateway, PaymentService>()`)
- [ ] Property and field access (`_db.Orders`) is not tracked
- [ ] Method groups and delegates (`list.ForEach(Process)`, `Click += OnClick`)
- [ ] Unresolved base types are always marked `Inherits`, because without the type we can't tell a class from an interface
- [ ] Primary constructors (`class OrderService(PaymentService p)`) and property injection aren't detected as `Injects`

**CLI**

- [ ] `Run("")` throws instead of returning `InvalidUsage`
- [ ] The symlink skipping in file discovery needs a test, which is easiest on Linux CI

**Done when:** all of steps 1–8 are merged, and running the CLI on a real open-source C# project gives a map with no duplicate IDs and no crashes.

---

## Milestone 2: Backend

**Goal:** upload a codebase, analyze it in the background, and store and query the map.

- **Database: PostgreSQL.** Tables: `users`, `projects`, `snapshots`, `source_files`, `symbols` (with a `stable_key`), `relations` (with a nullable target and a `confidence`). Schema changes go through migrations.
- **API:** create a project, upload a ZIP, list snapshots, and query the graph (neighbours of a symbol, filter by kind, search by name).
- **Background worker:** analysis never runs inside an API request. The API puts a job on a queue, and the worker runs the **same** `CodeMap.Analysis` library the CLI uses, then stores the result.
- **Safe uploads:** size and file-count limits, ZIP path traversal protection, no symlinks, no code execution.
- **Auth:** users only see their own projects.
- **Batch inserts:** a large repo can have 100,000+ symbols, so they shouldn't be written one row at a time.

## Milestone 3: Frontend

**Goal:** a map that people can explore.

- Upload a project and watch the analysis progress.
- Interactive graph: zoom, pan, filter by relation kind and confidence, search for a symbol.
- Type-level view by default (members rolled up through `containingId`), with drill-down into members.
- Symbol details panel: location, incoming and outgoing relations, a link to the source line.
- Clustering by namespace or folder. Drawing 50,000 nodes at once isn't usable.

## Milestone 4: More languages

Each language gets its own analyzer behind `ILanguageAnalyzer`, producing the same JSON contract.

| Language | Engine | Notes |
|---|---|---|
| TypeScript / JavaScript | TypeScript Compiler API (`ts.createProgram` + `TypeChecker`) | Runs in Node.js as a separate process that prints JSON. Mostly `Certain` relations. |
| Python | Pyright | Dynamic typing means many more `Unresolved` relations. May need an `Inferred` confidence level. |
| Others | tree-sitter | Syntax only, as a fallback: names and calls by name, all `Unresolved`. |

## Milestone 5: Scale and hardening

- **Multi-project solutions:** one compilation per project, with references between them.
- **Parallel analysis:** Roslyn compilations are immutable and safe to query from several threads.
- **Incremental analysis:** re-analyze only the changed files, using stable IDs to match symbols.
- **Snapshot diffs:** show what changed in the map between two commits.
- **CI:** build and test on Linux and Windows for every PR.
