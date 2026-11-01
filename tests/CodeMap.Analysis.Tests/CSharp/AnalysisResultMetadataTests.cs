using CodeMap.Analysis.Model; 

namespace CodeMap.Analysis.Tests.CSharp;

public sealed class AnalysisResultMetadataTests
{
    private readonly AnalysisResult _result = SnippetAnalysis.Run(("A.cs", "public class A { }"));
    [Fact]
    public void Analyze_Snippet_SetsCurrentSchemaVersion()
    {
        Assert.Equal(AnalysisResult.CurrentSchemaVersion, _result.SchemaVersion) ; 
        Assert.Equal(1, AnalysisResult.CurrentSchemaVersion) ;
    }
    [Fact]
    public void Analyze_Snippet_IdentifiesCSharpAnalyzer()
    {
        Assert.Equal("CodeMap.CSharp", _result.Analyzer.Name);
        Assert.Equal("csharp", _result.Analyzer.Language);
    }
}