
namespace CodeMap.Analysis.Tests.CSharp;

public sealed class LocationTests
{
    [Fact]
    public void Analyze_FileInSubfolder_UsesForwardSlashInFilePath()
    {
        var result = SnippetAnalysis.Run(
        (Path.Combine("Services", "OrderService.cs"), "public class OrderService { }"));
        var symbol = Assert.Single(result.Symbols);
        Assert.Equal("Services/OrderService.cs", symbol.Location.FilePath);
    }
}