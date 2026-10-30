using CodeMap.Analysis.Model;

namespace CodeMap.Analysis.Tests.CSharp;
public sealed class InjectionRelationTests : IClassFixture<ShopAnalysisFixture>
{
    private readonly AnalysisResult _result;

    public InjectionRelationTests(ShopAnalysisFixture fixture)
    {
        _result = fixture.Result;
    }
    [Theory]
    [InlineData("M:Shop.Services.OrderService.#ctor(Shop.Services.PaymentService,Shop.Data.AppDbContext)", "T:Shop.Services.PaymentService")]
    [InlineData("M:Shop.Services.OrderService.#ctor(Shop.Services.PaymentService,Shop.Data.AppDbContext)", "T:Shop.Data.AppDbContext")]
    [InlineData("M:Shop.Controllers.OrdersController.#ctor(Shop.Services.OrderService)", "T:Shop.Services.OrderService")]

    public void Analyze_ShopFixture_FindsInjectionRelation(string sourceId, string targetId)
    {
        Assert.Contains(_result.Relations, r => r.SourceId == sourceId &&
                                                r.TargetId == targetId &&
                                                r.Kind == RelationKind.Injects &&
                                                r.Confidence == Confidence.Certain);
    }

    [Fact]
    public void Analyze_ShopFixture_DoesNotTreatMethodParametersAsInjection()
    {
        Assert.DoesNotContain(_result.Relations, r => r.SourceId == "M:Shop.Services.OrderService.PlaceOrder(Shop.Models.Order)" &&
                                                r.Kind == RelationKind.Injects);
    }

    [Fact]
    public void Analyze_ShopFixture_MarksMissingInjectedTypeAsUnresolved()
    {
        Assert.Contains(_result.Relations, r => r.SourceId.StartsWith("M:Shop.Controllers.ProductsController.#ctor", StringComparison.Ordinal) &&
                                                r.TargetId == null &&
                                                r.TargetName == "ILogger<ProductsController>" &&
                                                r.Kind == RelationKind.Injects &&
                                                r.Confidence == Confidence.Unresolved);
    }

    
} 