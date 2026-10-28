using CodeMap.Analysis.CSharp;
using CodeMap.Analysis.Model;

namespace CodeMap.Analysis.Tests.CSharp;

public sealed class ShopAnalysisFixture
{

    public AnalysisResult Result { get; }
    public ShopAnalysisFixture()
    {
        var rootPath = Path.Combine(AppContext.BaseDirectory, "Fixtures", "Shop");
        var files = Directory.GetFiles(rootPath, "*.cs", SearchOption.AllDirectories);
        var analyzer = new CSharpAnalyzer();
        Result = analyzer.Analyze(rootPath, files);

    }
}
