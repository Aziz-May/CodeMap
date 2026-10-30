using System.Diagnostics.CodeAnalysis;

namespace CodeMap.Cli;

internal sealed class AnalyzeCommand
{
    [SuppressMessage("Performance", "CA1822:Mark members as static",
    Justification = "Becomes instance-based  when the analyzer is injected.")]
    public int Run(string path, TextWriter output, TextWriter error)
    {

       if (!Directory.Exists(path))
        {
            error.WriteLine($"Error: The specified path '{path}' does not exist.");
            return ExitCodes.PathNotFound;
        }
        return ExitCodes.Success;
    }
}