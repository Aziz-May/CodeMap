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
    [Theory]
    [InlineData("T:Shop.Services.IPaymentGateway", CodeSymbolKind.Interface)]
    [InlineData("M:Shop.Services.IPaymentGateway.Charge(System.Decimal)", CodeSymbolKind.Method)]
    [InlineData("T:Shop.Models.Money", CodeSymbolKind.Struct)]
    [InlineData("T:Shop.Models.OrderSummary", CodeSymbolKind.Record)]
    [InlineData("T:Shop.Models.OrderStatus", CodeSymbolKind.Enum)]
    [InlineData("M:Shop.Services.OrderService.#ctor(Shop.Services.PaymentService,Shop.Data.AppDbContext)", CodeSymbolKind.Constructor)]
    [InlineData("P:Shop.Models.Order.Total", CodeSymbolKind.Property)]
    [InlineData("F:Shop.Services.OrderService._paymentService", CodeSymbolKind.Field)]
    [InlineData("F:Shop.Services.PaymentService._successCount", CodeSymbolKind.Field)]
    [InlineData("F:Shop.Services.PaymentService._failureCount", CodeSymbolKind.Field)]


    public void Analyze_ShopFixture_FindsSymbolWithExpectedKind(string expectedId, CodeSymbolKind expectedKind)
    {
        Assert.Contains(_result.Symbols, s => s.Id == expectedId && s.Kind == expectedKind);
    }
    [Fact]
    public void Analyze_ShopFixture_UsesTypeNameForConstructorName()
    {
        var constructor = Assert.Single(_result.Symbols, s => s.Id == "M:Shop.Services.OrderService.#ctor(Shop.Services.PaymentService,Shop.Data.AppDbContext)");
        Assert.Equal("OrderService", constructor.Name);
    }
}
