using CodeMap.Analysis.Model;

namespace CodeMap.Analysis.Tests.CSharp;

public sealed class GenericAndExtensionCallTests
{
    [Fact] 
    public void Analyze_GenericMethodCall_TargetsDeclaredMethod()
    {
        var result = SnippetAnalysis.Run(("Repo.cs", 
        """
        public class Repo 
        {
        public T Get<T>()=> default!; 
        public void MethodA()
        {
          new Repo().Get<int>();
          }
        }
        """)
        );
       var get = Assert.Single(result.Symbols, s => s.Name == "Get");
        Assert.Contains(result.Relations, r => r.Kind == RelationKind.Calls && r.TargetId == get.Id);

    }
    [Fact] 
    public void Analyze_ExtensionMethodCall_TargetsDeclaredMethod()
    {
        var result = SnippetAnalysis.Run(("OrderExtensions.cs", 
        """
        public static class OrderExtensions
        {
            public static int Twice(this int x) => x * 2;
            public static void MethodA()
            {
               
                int y = 5.Twice();
            }
        }
        """)
        );
        var twice = Assert.Single(result.Symbols, s => s.Name == "Twice");
        Assert.Contains(result.Relations, r => r.Kind == RelationKind.Calls && r.TargetId == twice.Id);

        
    }
    [Fact]
    public void Analyze_GenericTypeInstantiation_TargetsDeclaredType()
    {
        var result = SnippetAnalysis.Run(("Box.cs", 
        """
        public class Box<T> 
        {
            public void MethodA()
            {
                var r = new Box<int>();
            }
        }
        """)
        );
       var box = Assert.Single(result.Symbols, s => s.Name == "Box");
        Assert.Contains(result.Relations, r => r.Kind == RelationKind.Instantiates && r.TargetId == box.Id);

    }
    [Fact]
    public void Analyze_CallOnGenericTypeMember_TargetsDeclaredMethod()
    {
        var result = SnippetAnalysis.Run(("Box.cs", 
        """
        public class Box<T> 
        {
            public T Take(T item) => item;
            public void MethodA()
            {
                var r = new Box<int>().Take(5);
                
            }
            
        }
        """)
        );
       var take = Assert.Single(result.Symbols, s => s.Name == "Take");
        Assert.Contains(result.Relations, r => r.Kind == RelationKind.Calls && r.TargetId == take.Id);

    }
    
}