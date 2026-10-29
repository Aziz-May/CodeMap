using CodeMap.Analysis.Model;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CodeMap.Analysis.CSharp;

public sealed class CSharpAnalyzer : ILanguageAnalyzer
{
    public AnalysisResult Analyze(string rootPath, IReadOnlyList<string> files, CancellationToken cancellationToken = default)
    {
        var trees = new List<SyntaxTree>();
        foreach (var file in files)
        {
            var text = File.ReadAllText(file);
            var tree = CSharpSyntaxTree.ParseText(text, path: file, cancellationToken: cancellationToken);
            trees.Add(tree);
        }
        var compilation = CSharpCompilation.Create("CodeMapAnalysis", trees, GetPlatformReferences());
        var symbols = new List<CodeSymbol>();
        var relations = new List<CodeRelation>();
        foreach (var tree in trees)
        {
            var model = compilation.GetSemanticModel(tree);
            var symbolCollector = new SymbolCollector(model, rootPath, cancellationToken);
            symbolCollector.Visit(tree.GetRoot(cancellationToken));
            symbols.AddRange(symbolCollector.Symbols);
            var relationCollector = new RelationCollector(model, rootPath, cancellationToken);
            relationCollector.Visit(tree.GetRoot(cancellationToken));
            relations.AddRange(relationCollector.Relations);

        }
        return new AnalysisResult(symbols, relations, []);
    }
    private static List<PortableExecutableReference> GetPlatformReferences()
    {
        var paths = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string ?? "";
        return paths
        .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
        .Select(path => MetadataReference.CreateFromFile(path))
        .ToList();
    }
}
