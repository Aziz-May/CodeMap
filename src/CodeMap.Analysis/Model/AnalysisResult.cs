namespace CodeMap.Analysis.Model;

public record AnalysisResult(
    IReadOnlyList<CodeSymbol> Symbols,
    IReadOnlyList<CodeRelation> Relations,
    IReadOnlyList<string> Warnings
);
