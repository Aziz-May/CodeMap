using CodeMap.Analysis.Tests; 
using CodeMap.Analysis.Model;

namespace CodeMap.Analysis.Tests;

public sealed class SnippetAnalysisTests
{
    [Fact]
    public void Run_SingleClass_FindsIt()
    {
        var result = SnippetAnalysis.Run(("A.cs", "public class A { }"));
        Assert.Contains(result.Symbols, s => s.Id == "T:A" && s.Kind == CodeSymbolKind.Class);
    }
}

