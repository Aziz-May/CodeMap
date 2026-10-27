using CodeMap.Analysis.Model;
using CodeMap.Analysis.CSharp;

namespace CodeMap.Analysis.Tests.CSharp;

public sealed class CSharpAnalyzerTests
{
    [Fact]
    public void Analyze_ShopFixture_FindsOrderServiceClass()
    {
         //Arrange
        var rootPath = Path.Combine(AppContext.BaseDirectory, "Fixtures", "Shop");
        var files = Directory.GetFiles(rootPath, "*.cs", SearchOption.AllDirectories);
        var analyzer = new CSharpAnalyzer(); 
         //Act
        var result = analyzer.Analyze(rootPath, files); 
        //Assert
        Assert.Contains(result.Symbols, s => s.Id == "T:Shop.Services.OrderService" && s.Kind == CodeSymbolKind.Class);
        
    }
    [Fact]
    public void Analyze_ShopFixture_FindsPlaceOrderMethodInsideOrderService(){
        
        //Arrange 
        var rootPath = Path.Combine(AppContext.BaseDirectory, "Fixtures", "Shop");
        var files = Directory.GetFiles(rootPath, "*.cs", SearchOption.AllDirectories);
        var analyzer = new CSharpAnalyzer(); 
         //Act
        var result = analyzer.Analyze(rootPath, files); 
        //Assert
        Assert.Contains(result.Symbols, s => s.Id == "M:Shop.Services.OrderService.PlaceOrder(Shop.Models.Order)" && s.Kind == CodeSymbolKind.Method && s.ContainingId == "T:Shop.Services.OrderService");
    }
}