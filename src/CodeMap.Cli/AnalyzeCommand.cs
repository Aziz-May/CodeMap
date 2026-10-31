using System.Diagnostics.CodeAnalysis;
using CodeMap.Analysis;

namespace CodeMap.Cli;

internal sealed class AnalyzeCommand
{
    private readonly ILanguageAnalyzer _analyzer;
    public AnalyzeCommand(ILanguageAnalyzer analyzer)
    {
        _analyzer = analyzer;
    }
    [SuppressMessage("Performance", "CA1822:Mark members as static",
    Justification = "Becomes instance-based  when the analyzer is injected.")]
    public int Run(string path, TextWriter output, TextWriter error)
    {
        var fullPath = Path.GetFullPath(path);

       if (!Directory.Exists(fullPath))
        {
            error.WriteLine($"Error: The specified path '{fullPath}' does not exist.");
            return ExitCodes.PathNotFound;
        }
        return ExitCodes.Success;
    }
}