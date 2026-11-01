namespace CodeMap.Analysis.Model;

public record AnalysisResult(
    IReadOnlyList<CodeSymbol> Symbols,
    IReadOnlyList<CodeRelation> Relations,
    IReadOnlyList<string> Warnings
)
{
    public const int CurrentSchemaVersion = 1 ; 
    public int SchemaVersion { get; init; } = CurrentSchemaVersion;
    public required AnalyzerInfo Analyzer { get; init; }
}
