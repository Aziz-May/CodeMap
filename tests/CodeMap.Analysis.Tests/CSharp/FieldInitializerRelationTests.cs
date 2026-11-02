using CodeMap.Analysis.Model;
namespace CodeMap.Analysis.Tests.CSharp;

public sealed class FieldInitializerRelationTests
{
    [Fact]
    public void Analyze_FieldInitializer_RecordsInstantiationFromField()
    {
        var result = SnippetAnalysis.Run(("Controller.cs", 
        """
        public class Service {}
        public class Controller
        {
            private readonly Sevice _service = new Service();}
        """
        ));
        Assert.Contains(result.Relations, r =>
            r.Kind == RelationKind.Instantiates &&
            r.SourceId == "F:Controller._service" &&
            r.TargetId == "T:Service");
    }

    [Fact]
    public void Analyze_FieldDeclarationWithTwoVariables_RecordsEachFromItsOwnField()
    {
        var result = SnippetAnalysis.Run(("Controller.cs", 
        """
        public class Service {}
        public class Controller
        {
            private readonly Service _service1 = new Service(), _service2 = new Service();
        }
        """
        ));
        Assert.Contains(result.Relations, r =>
            r.Kind == RelationKind.Instantiates &&
            r.SourceId == "F:Controller._service1" &&
            r.TargetId == "T:Service");
        Assert.Contains(result.Relations, r =>
            r.Kind == RelationKind.Instantiates &&
            r.SourceId == "F:Controller._service2" &&
            r.TargetId == "T:Service");
    }

    [Fact]
    public void Analyze_PropertyInitializer_RecordsInstantiationFromProperty()
    {
        var result = SnippetAnalysis.Run(("Controller.cs", 
        """
        public class Service {}
        public class Controller
        {
            private Service Service { get; } = new Service();
        }
        """
        ));
        Assert.Contains(result.Relations, r =>
            r.Kind == RelationKind.Instantiates &&
            r.SourceId == "P:Controller.Service" &&
            r.TargetId == "T:Service");
    }
    [Fact]
public void Analyze_LocalInsideFieldInitializerLambda_RecordsFromField()
{
    var result = SnippetAnalysis.Run(("Controller.cs", """
        using System;

        public class Service { }

        public class Controller
        {
            private readonly Func<Service> _factory = () =>
            {
                var s = new Service();
                return s;
            };
        }
        """));

    Assert.Contains(result.Relations, r =>
        r.Kind == RelationKind.Instantiates &&
        r.SourceId == "F:Controller._factory" &&
        r.TargetId == "T:Service");
}

}