using CodeMap.Analysis.Model;
namespace CodeMap.Analysis.Tests.CSharp ; 

public sealed class InterfaceInheritanceKindTests
{
    [Fact]
    public void Analyze_InterfaceInheritsInterface_ReportsInheritsRelation()
    {
        var result = SnippetAnalysis.Run(
        ("IBase.cs", "public interface IBase { }"),
        ("IDerived.cs", "public interface IDerived : IBase { }"));
        var relation = Assert.Single(result.Relations);
        Assert.Equal(RelationKind.Inherits, relation.Kind);
        Assert.Equal("T:IDerived", relation.SourceId);
        Assert.Equal("T:IBase", relation.TargetId);
    }
}