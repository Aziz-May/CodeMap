using CodeMap.Analysis.CSharp;
using CodeMap.Analysis.Model;
using CodeMap.Analysis.Discovery;

namespace CodeMap.Analysis.Tests.CSharp;

public sealed class ShopAnalysisFixture
{

    public AnalysisResult Result { get; }
    public ShopAnalysisFixture()
    {
        var rootPath = Path.Combine(AppContext.BaseDirectory, "Fixtures", "Shop");
        var files = SourceFileDiscovery.FindFiles(rootPath, ".cs");
        var analyzer = new CSharpAnalyzer();
        Result = analyzer.Analyze(rootPath, files);

    }
}
