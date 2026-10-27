namespace CodeMap.Analysis.Model;

public record CodeSymbol(
    string Id,
    string Name,
    CodeSymbolKind Kind,
    string? ContainingId,
    CodeLocation Location
   );
