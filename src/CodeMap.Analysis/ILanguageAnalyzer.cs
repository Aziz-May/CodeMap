using CodeMap.Analysis.Model;
namespace CodeMap.Analysis;



public interface ILanguageAnalyzer
{

    AnalysisResult Analyze(string rootPath, IReadOnlyList<string> files, CancellationToken cancellationToken = default);
};
