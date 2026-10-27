namespace CodeMap.Analysis.Model;

public record CodeLocation(
        string FilePath,
        int StartLine,
        int EndLine
     );
