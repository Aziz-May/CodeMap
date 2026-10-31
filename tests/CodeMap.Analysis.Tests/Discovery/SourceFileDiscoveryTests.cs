using CodeMap.Analysis.Discovery;

namespace CodeMap.Analysis.Tests.Discovery;

public sealed class SourceFileDiscoveryTests : IDisposable
{
    private readonly string _root;
    public SourceFileDiscoveryTests()
    {
        _root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        CreateFile("Order.cs");
        CreateFile(Path.Combine("Services", "PaymentService.cs"));
        CreateFile(Path.Combine("bin", "Debug", "Old.cs"));
        CreateFile(Path.Combine("obj", "Generated.cs"));
        CreateFile(Path.Combine(".git", "hooks", "x.cs"));
        CreateFile(Path.Combine("node_modules", "pkg", "y.cs"));
        CreateFile(Path.Combine("Views", "Form1.Designer.cs"));
        CreateFile("notes.md");
        CreateFile(Path.Combine("Models", "Order.g.cs"));

    }
    public void Dispose()
    {
        Directory.Delete(_root , recursive : true);
    }
    private void CreateFile(string relativePath)
    {
        var fullPath = Path.Combine(_root , relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        File.WriteAllText(fullPath, "// test");
    }

    [Fact]
    public void FindFiles_FindsCsFilesInRootAndSubfolders()
    {
        var files = SourceFileDiscovery.FindFiles(_root, ".cs");
        Assert.Contains(Path.Combine(_root, "Order.cs"), files);
        Assert.Contains(Path.Combine(_root, "Services", "PaymentService.cs"), files);

    }
    [Theory]
    [InlineData("bin")]
    [InlineData("obj")]
    [InlineData(".git")]
    [InlineData("node_modules")]

    public void FindFiles_SkipsIgnoredFolders(string folder)
    {
        var segment = Path.DirectorySeparatorChar + folder + Path.DirectorySeparatorChar;
        var files = SourceFileDiscovery.FindFiles(_root, ".cs");    
        Assert.DoesNotContain(files, f => f.Contains(segment, StringComparison.OrdinalIgnoreCase));
    }
    [Theory]
    [InlineData("Form1.Designer.cs")]
    [InlineData("Order.g.cs")]
    public void FindFiles_SkipsGeneratedFiles(string fileName)
    {
        var files = SourceFileDiscovery.FindFiles(_root, ".cs");
        Assert.DoesNotContain(files, f => Path.GetFileName(f).Equals(fileName, StringComparison.OrdinalIgnoreCase));
    }
    [Fact]
    public void FindFiles_ReturnsOnlyRequestedExtension()
    {
        var files = SourceFileDiscovery.FindFiles(_root, ".cs");
        Assert.All(files, f=> Assert.EndsWith(".cs", f, StringComparison.OrdinalIgnoreCase));
    }
    [Fact]
    public void FindFiles_ReturnsFilesInSortedOrder()
    {
        var files = SourceFileDiscovery.FindFiles(_root, ".cs");
        Assert.Equal(files.Order(StringComparer.Ordinal), files);
    }
}