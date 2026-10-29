using CodeMap.Analysis.Model;

namespace CodeMap.Analysis.Tests.CSharp; 

public sealed class InstantiationRelationTests : IClassFixture<ShopAnalysisFixture>
{
    private readonly AnalysisResult _result;

    public InstantiationRelationTests(ShopAnalysisFixture fixture)
    {
        _result = fixture.Result;
    }
    [Theory]
    [InlineData("M:Shop.Program.Main","T:Shop.Data.AppDbContext")]
    [InlineData("M:Shop.Program.Main","T:Shop.Services.PaymentService")]
    [InlineData("M:Shop.Program.Main","T:Shop.Services.OrderService")]
    [InlineData("M:Shop.Program.Main","T:Shop.Models.Order")]

    public void Analyze_ShopFixture_FindsInstantiationRelation(string sourceId, string targetId)
    {
        Assert.Contains(_result.Relations, r => r.SourceId == sourceId &&
                                                r.TargetId == targetId &&
                                                r.Kind == RelationKind.Instantiates &&
                                                r.Confidence == Confidence.Certain);
    }
    [Fact] 
    public void Analyze_ShopFixture_SkipsInstantiationOfDotNetTypes()
    {
       Assert.DoesNotContain(_result.Relations, r => r.SourceId == "P:Shop.Data.AppDbContext.Orders" &&
                                              r.Kind == RelationKind.Instantiates);

    }
    

    

}