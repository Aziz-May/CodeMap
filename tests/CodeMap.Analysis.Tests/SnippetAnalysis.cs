using CodeMap.Analysis.CSharp; 
using CodeMap.Analysis.Model;

namespace CodeMap.Analysis.Tests; 

internal static class SnippetAnalysis
{
    public static AnalysisResult Run(params (string RelativePath, string Code)[] files)
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        try
        {
            
            var paths = new List<string>();
            foreach (var file in files)
            {
                var fullPath = Path.Combine(root, file.RelativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
                File.WriteAllText(fullPath, file.Code);
                paths.Add(fullPath);
            }
               

           
            return new CSharpAnalyzer().Analyze(root, paths);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }
    
}