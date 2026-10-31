
using CodeMap.Analysis;
using CodeMap.Analysis.Serialization;

namespace CodeMap.Cli;

internal sealed class AnalyzeCommand
{
    private readonly ILanguageAnalyzer _analyzer;
    public AnalyzeCommand(ILanguageAnalyzer analyzer)
    {
        _analyzer = analyzer;
    }
   
    public int Run(string path, TextWriter output, TextWriter error)
    {
        var fullPath = Path.GetFullPath(path);

       if (!Directory.Exists(fullPath))
        {
            error.WriteLine($"Error: The specified path '{fullPath}' does not exist.");
            return ExitCodes.PathNotFound;
        }
        var files = Directory.GetFiles(fullPath, "*.cs", SearchOption.AllDirectories);
        var result = _analyzer.Analyze(fullPath, files);
        output.Write(AnalysisJson.Serialize(result));
        return ExitCodes.Success;
    }
}