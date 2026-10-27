using CodeMap.Analysis.Model;
namespace CodeMap.Analysis.CSharp;

public sealed class CSharpAnalyzer : ILanguageAnalyzer
{
    public AnalysisResult Analyze(string rootPath, IReadOnlyList<string> files, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}