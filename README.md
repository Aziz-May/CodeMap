# CodeMap

CodeMap reads a codebase and builds a **dependency map** from it: which types and members exist, and how they call, create, inherit from, implement and inject each other.

It reads the code **semantically**, not as text. For C#, that means [Roslyn](https://github.com/dotnet/roslyn), the real C# compiler. So when a method calls `_paymentService.Charge(...)`, CodeMap knows *which* `Charge` it is, even if three classes have a method with that name.

> **Status:** Milestone 1 (the analysis engine + CLI) is nearly done. The next goal is a graph accurate enough for trustworthy **change impact analysis**, built and proven in the CLI before any backend or web UI. See [docs/ROADMAP.md](docs/ROADMAP.md).

---

## Quick start

**Requirements:** [.NET SDK 10.0.403](https://dotnet.microsoft.com/download) or a later 10.0 feature band (pinned in [global.json](global.json)).

```bash
git clone https://github.com/Aziz-May/CodeMap.git
cd CodeMap
dotnet build
dotnet test

# Analyze a folder of C# code and print the map as JSON
dotnet run --project src/CodeMap.Cli -- analyze tests/Fixtures/Shop

# Save it to a file (stdout carries only the JSON, so redirecting is safe)
dotnet run --project src/CodeMap.Cli -- analyze tests/Fixtures/Shop > map.json
```

### Exit codes

| Code | Meaning |
|---|---|
| `0` | Success |
| `1` | Invalid usage (e.g. a missing argument) |
| `2` | The path does not exist |

Errors and messages go to **stderr**, and **stdout** only ever contains the JSON result.

---

## What the output looks like

Running the CLI on the sample `Shop` fixture finds **41 symbols** and **19 relations**. Here is a shortened excerpt:

```json
{
  "symbols": [
    {
      "id": "T:Shop.Services.OrderService",
      "name": "OrderService",
      "kind": "Class",
      "containingId": null,
      "location": { "filePath": "Services/OrderService.cs", "startLine": 6, "endLine": 27 }
    }
  ],
  "relations": [
    {
      "sourceId": "M:Shop.Services.OrderService.#ctor(Shop.Services.PaymentService,Shop.Data.AppDbContext)",
      "targetId": "T:Shop.Services.PaymentService",
      "targetName": "PaymentService",
      "kind": "Injects",
      "confidence": "Certain",
      "location": { "filePath": "Services/OrderService.cs", "startLine": 11, "endLine": 11 }
    },
    {
      "sourceId": "M:Shop.Controllers.ProductsController.GetAll",
      "targetId": null,
      "targetName": "_logger.LogInformation",
      "kind": "Calls",
      "confidence": "Unresolved",
      "location": { "filePath": "Controllers/ProductsController.cs", "startLine": 24, "endLine": 24 }
    }
  ],
  "warnings": []
}
```

`filePath` is relative to the analyzed folder and always uses `/`, on every OS.

### Symbols

A **symbol** is anything declared in the code: classes, interfaces, structs, records, enums, methods, constructors, properties and fields.

Each symbol has a **stable ID** in Roslyn's [documentation comment ID](https://learn.microsoft.com/dotnet/csharp/language-reference/xmldoc/#id-strings) format:

| Prefix | Kind | Example |
|---|---|---|
| `T:` | type | `T:Shop.Services.OrderService` |
| `M:` | method / constructor | `M:Shop.Services.OrderService.PlaceOrder(Shop.Models.Order)` |
| `P:` | property | `P:Shop.Models.Order.Total` |
| `F:` | field | `F:Shop.Services.OrderService._paymentService` |

The ID depends only on the code itself, not on line numbers or file names, so the same symbol keeps the same ID when the code around it changes. `containingId` links a member to the type it belongs to.

### Relations

| Kind | Meaning | Example in the code |
|---|---|---|
| `Calls` | a member invokes a method | `_paymentService.Charge(order.Total)` |
| `Instantiates` | a member creates an object | `new OrderService(payment, db)` or `new()` |
| `Injects` | a constructor receives a dependency | `public OrderService(PaymentService paymentService)` |
| `Inherits` | a type extends a base class | `class Product : Entity` |
| `Implements` | a type implements an interface | `class PaymentService : IPaymentGateway` |

The **source** of a relation is always the innermost member where the code appears (for example the method that makes the call). A type-level view ("`OrderService` depends on `PaymentService`") can be built by rolling relations up through `containingId`.

### Confidence

| Value | Meaning |
|---|---|
| `Certain` | Roslyn resolved the target to a symbol in the analyzed code, so `targetId` is set. |
| `Unresolved` | The target couldn't be resolved, for example because it comes from a NuGet package that isn't available during analysis. `targetId` is `null` and `targetName` holds the text written in the code. |

Calls into the .NET libraries themselves (`List.Add`, `Console.WriteLine`, …) are **skipped on purpose**: they are noise in a map of *your* code.

---

## How it works

```
 path ──► find *.cs files ──► parse each file ──► one compilation ──► per file:      ──► AnalysisResult ──► JSON
                              (syntax trees)      (+ .NET refs)       SymbolCollector     (symbols,
                                                                      RelationCollector    relations,
                                                                                           warnings)
```

1. **Parse.** Every file becomes a Roslyn `SyntaxTree`.
2. **Compile.** All trees go into one `CSharpCompilation`, with references to the .NET platform assemblies, so Roslyn can resolve types across files.
3. **Walk.** For each tree, two `CSharpSyntaxWalker`s (the Visitor pattern) visit the code using the tree's `SemanticModel`:
   - `SymbolCollector` records every declaration.
   - `RelationCollector` records every call, `new`, base type and constructor parameter. All five kinds go through a single `AddRelation` method that applies the same rules: unresolved → `Unresolved`, external .NET → skipped, in source → `Certain`.
4. **Serialize.** `AnalysisJson` writes the result as camelCase JSON, with enums as strings.

### Security note

CodeMap never builds or runs the analyzed code. It does **not** use `MSBuildWorkspace`, which would evaluate the project's MSBuild files and could run arbitrary code from an untrusted upload. Instead, it builds the compilation directly from the source files.

---

## Project layout

```
CodeMap/
├── src/
│   ├── CodeMap.Analysis/            The engine (a library, reusable by the CLI and the future backend)
│   │   ├── ILanguageAnalyzer.cs     Contract every language analyzer implements
│   │   ├── Model/                   AnalysisResult, CodeSymbol, CodeRelation, CodeLocation + enums
│   │   ├── CSharp/                  Roslyn implementation
│   │   │   ├── CSharpAnalyzer.cs    Parses, compiles, runs the collectors
│   │   │   ├── SymbolCollector.cs   Declarations → symbols
│   │   │   ├── RelationCollector.cs Calls / new / base types / injection → relations
│   │   │   └── SyntaxLocations.cs   Syntax node → CodeLocation (relative path, 1-based lines)
│   │   └── Serialization/
│   │       └── AnalysisJson.cs      The JSON contract (shared serializer options)
│   └── CodeMap.Cli/                 Command-line entry point
│       ├── Program.cs               Composition root: System.CommandLine wiring only
│       ├── AnalyzeCommand.cs        The `analyze` logic, testable without a console
│       └── ExitCodes.cs
├── tests/
│   ├── CodeMap.Analysis.Tests/      Engine tests, run against the Shop fixture
│   ├── CodeMap.Cli.Tests/           CLI tests, using a fake analyzer and temp folders
│   └── Fixtures/Shop/               A small sample codebase used as test input
├── docs/ROADMAP.md                  The plan
├── Directory.Build.props            Shared build settings (net10.0, nullable, analyzers as errors)
├── Directory.Packages.props         Central Package Management: all NuGet versions live here
├── global.json                      Pins the .NET SDK
└── CodeMap.slnx
```

---

## Design decisions

| Decision | Why |
|---|---|
| **`ILanguageAnalyzer` (Strategy pattern)** | C# is the first language. TypeScript and Python analyzers will plug in behind the same interface, and nothing above it changes. |
| **The model is plain records with string IDs** | No Roslyn types leak out, so results serialize cleanly and can cross process or machine boundaries (external analyzers, a background worker). |
| **Stable symbol IDs** | They make it possible to compare snapshots of the same codebase over time. |
| **An explicit `Confidence`** | The tool says when it doesn't know instead of silently dropping or guessing. Dynamic languages will rely on this. |
| **One `AddRelation` path** | All relation kinds share the same resolution rules, so a fix applies everywhere at once. |
| **A thin CLI** | `Program.cs` only wires things together. The logic lives in classes that receive their dependencies (`ILanguageAnalyzer`, `TextWriter`s), which makes them testable and reusable by the backend. |
| **JSON enums as strings, camelCase names** | Reordering an enum can't silently change the meaning of stored data, and the format suits JavaScript consumers. |

---

## Development

```bash
dotnet build      # analyzer warnings are errors (TreatWarningsAsErrors + latest-recommended)
dotnet test       # all tests must pass before merging
```

**Workflow:**

- **TDD.** Write failing tests, make them pass, refactor, then merge.
- **One feature branch and one pull request per step.** `main` is always green.
- **NuGet versions are set only in `Directory.Packages.props`.** Never put a `Version=` on a `PackageReference`.
- Known gaps and bugs are tracked as [GitHub Issues](https://github.com/Aziz-May/CodeMap/issues).

## Known limitations

The engine covers the common cases; some C# features are not handled yet (partial classes, generic and extension method IDs, top-level statements, local functions, type usage as a dependency…). The full list and the plan for fixing them are in [docs/ROADMAP.md](docs/ROADMAP.md).
