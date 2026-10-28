using CodeMap.Analysis.CSharp;
using CodeMap.Analysis.Model;

namespace CodeMap.Analysis.Tests.CSharp;

public sealed class CSharpAnalyzerTests : IClassFixture<ShopAnalysisFixture>
{
    private readonly AnalysisResult _result;
    public CSharpAnalyzerTests(ShopAnalysisFixture fixture)
    {
        _result = fixture.Result;
    }
    [Fact]
    public void Analyze_ShopFixture_FindsOrderServiceClass()
    {


        Assert.Contains(_result.Symbols, s => s.Id == "T:Shop.Services.OrderService" && s.Kind == CodeSymbolKind.Class);

    }
    [Fact]
    public void Analyze_ShopFixture_FindsPlaceOrderMethodInsideOrderService()
    {


        Assert.Contains(_result.Symbols, s => s.Id == "M:Shop.Services.OrderService.PlaceOrder(Shop.Models.Order)" && s.Kind == CodeSymbolKind.Method && s.ContainingId == "T:Shop.Services.OrderService");
    }
}
