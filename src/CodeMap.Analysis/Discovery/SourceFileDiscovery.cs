namespace CodeMap.Analysis.Discovery; 

public static class SourceFileDiscovery
{
    private static readonly HashSet<string> IgnoredFolders = new(StringComparer.OrdinalIgnoreCase)
    {
        ".git",
        ".vs",
        "bin",
        "obj",
        ".idea",
        "node_modules",
        "packages",
        "TestResults"
    };
    private static readonly EnumerationOptions Options = new()
    { AttributesToSkip = FileAttributes.ReparsePoint ,
    IgnoreInaccessible = true,
    };
    public static IReadOnlyList<string> FindFiles(string rootPath, string extension)
    {
        var result = new List<string>();
        var pending = new Stack<string>();
        pending.Push(rootPath);
        while (pending.Count > 0)
        {
            var directory = pending.Pop();
            foreach (var file in Directory.EnumerateFiles(directory, "*" + extension, Options))
            {
                if (!IsGenerated(file))
                {
                    result.Add(file);
                }
            }
            foreach (var subdirectory in Directory.EnumerateDirectories(directory, "*", Options))
            {
                if (!IgnoredFolders.Contains(Path.GetFileName(subdirectory)))
                {
                    pending.Push(subdirectory);
                }
            }
        }
        result.Sort(StringComparer.Ordinal);
        return result;
        
    }
    private static bool IsGenerated(string file)
    {
        var name = Path.GetFileNameWithoutExtension(file);
        return name.EndsWith(".g", StringComparison.OrdinalIgnoreCase) ||
               name.EndsWith(".designer", StringComparison.OrdinalIgnoreCase) ;
    }
}