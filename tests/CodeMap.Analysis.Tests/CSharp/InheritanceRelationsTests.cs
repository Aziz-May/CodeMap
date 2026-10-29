using CodeMap.Analysis.Model;

namespace CodeMap.Analysis.Tests.CSharp;

public sealed class InheritanceRelationsTests : IClassFixture<ShopAnalysisFixture>
{
   private readonly AnalysisResult _result;

    public InheritanceRelationsTests(ShopAnalysisFixture fixture)
    {

        _result = fixture.Result;
    }
    [Theory]
    [InlineData("T:Shop.Services.PaymentService", "T:Shop.Services.IPaymentGateway", RelationKind.Implements)]
    [InlineData("T:Shop.Models.Product", "T:Shop.Models.Entity", RelationKind.Inherits)]
    public void Analyze_ShopFixture_FindsBaseTypeRelation(string sourceId, string targetId, RelationKind expectedKind)
    {
        Assert.Contains(_result.Relations, r => r.SourceId == sourceId &&
                                                r.TargetId == targetId &&
                                                r.Kind == expectedKind &&
                                                r.Confidence == Confidence.Certain);
    }
    [Fact] 
    public void Analyze_ShopFixture_DoesNotMarkInterfaceAsInherits()
    {
        Assert.DoesNotContain(_result.Relations, r => r.SourceId == "T:Shop.Services.PaymentService" &&
                                                r.TargetId == "T:Shop.Services.IPaymentGateway" &&
                                                r.Kind == RelationKind.Inherits);
    }
    [Fact]
    public void Analyze_ShopFixture_MarksMissingBaseClassAsUnresolved()
    {
        Assert.Contains(_result.Relations, r =>
            r.SourceId == "T:Shop.Controllers.ProdcutsController" &&
            r.TargetId == null &&
            r.TargetName == "ControllerBase" &&
            r.Kind == RelationKind.Inherits &&
            r.Confidence == Confidence.Unresolved);
    }

    
    

}

    