using CodeMap.Analysis.Model;



namespace CodeMap.Analysis.Tests.CSharp;

public sealed class CallRelationTests : IClassFixture<ShopAnalysisFixture>
{
    private readonly AnalysisResult _result;

    public CallRelationTests(ShopAnalysisFixture fixture)
    {

        _result = fixture.Result;
    }
    [Theory]
    [InlineData("M:Shop.Services.OrderService.PlaceOrder(Shop.Models.Order)", "M:Shop.Services.PaymentService.Charge(System.Decimal)")]
    [InlineData("M:Shop.Controllers.OrdersController.Create(Shop.Models.Order)", "M:Shop.Services.OrderService.PlaceOrder(Shop.Models.Order)")]
    [InlineData("M:Shop.Program.Main", "M:Shop.Controllers.OrdersController.Create(Shop.Models.Order)")]
    [InlineData("M:Shop.Services.OrderService.PlaceOrder(Shop.Models.Order)", "M:Shop.Data.AppDbContext.SaveChanges")]
    public void Analyze_ShopFixture_FindsCallRelation(string sourceId, string targetId)
    {
        Assert.Contains(_result.Relations, r => r.SourceId == sourceId &&
                                                r.TargetId == targetId &&
                                                r.Kind == RelationKind.Calls &&
                                                r.Confidence == Confidence.Certain);
    }

    [Fact]
    public void Analyze_ShopFixture_RecordsChargeCallExactlyOnce()
    {
        Assert.Single(_result.Relations, r => r.SourceId == "M:Shop.Services.OrderService.PlaceOrder(Shop.Models.Order)" &&
                                            r.TargetId == "M:Shop.Services.PaymentService.Charge(System.Decimal)" && 
                                            r.TargetName == "PaymentService.Charge" );
    }
    [Fact]
    public void Analyze_ShopFixture_SkipsCallsIntoDotNetLibraries()
    {
        var callsFromPlaceOrder = _result.Relations.Count(r => r.SourceId == "M:Shop.Services.OrderService.PlaceOrder(Shop.Models.Order)" && r.Kind == RelationKind.Calls);
        Assert.Equal(2, callsFromPlaceOrder);
    }
    [Fact]
    public void Analyze_ShopFixture_MarksCallIntoMissingPackageAsUnresolved()
    {
        Assert.Contains(_result.Relations, r =>
            r.SourceId == "M:Shop.Controllers.ProductsController.GetAll" &&
            r.TargetId == null &&
            r.TargetName == "_logger.LogInformation" &&
            r.Confidence == Confidence.Unresolved);
    }




}
