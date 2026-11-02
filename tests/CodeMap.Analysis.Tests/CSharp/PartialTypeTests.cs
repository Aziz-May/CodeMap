namespace CodeMap.Analysis.Tests.CSharp;

public sealed class PartialTypeTests
{
    [Fact]
    public void Analyze_PartialClassInTwoFiles_ProducesOneSymbol()
    {
        var result = SnippetAnalysis.Run(
            ("Order.cs", "public partial class Order { public void A() { } }"),
            ("Order.More.cs", "public partial class Order { public void B() { } }")
        );
        var order = Assert.Single(result.Symbols, s => s.Id == "T:Order");
        Assert.Equal("Order.cs", order.Location.FilePath);

    }
}