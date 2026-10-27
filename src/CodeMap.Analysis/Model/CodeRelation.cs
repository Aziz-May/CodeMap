namespace CodeMap.Analysis.Model;

public record CodeRelation(
    string SourceId,
    string? TargetId,
    string TargetName,
    RelationKind Kind,
    Confidence Confidence,
    CodeLocation Location
);
