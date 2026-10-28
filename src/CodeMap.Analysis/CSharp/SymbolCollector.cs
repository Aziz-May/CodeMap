using CodeMap.Analysis.Model;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeMap.Analysis.CSharp;


internal sealed class SymbolCollector : CSharpSyntaxWalker
{

    private readonly SemanticModel _model;
    private readonly string _rootPath;
    private readonly CancellationToken _cancellationToken;
    private readonly List<CodeSymbol> _symbols = [];

    public SymbolCollector(SemanticModel model, string rootPath, CancellationToken cancellationToken = default)
    {
        _model = model;
        _rootPath = rootPath;
        _cancellationToken = cancellationToken;
    }

    public IReadOnlyList<CodeSymbol> Symbols => _symbols;

    private void AddSymbol(SyntaxNode node, CodeSymbolKind kind)
    {

        var symbol = _model.GetDeclaredSymbol(node, _cancellationToken);
        var id = symbol?.GetDocumentationCommentId();
        if (symbol is null || id is null) return;
        var span = node.GetLocation().GetLineSpan();
        var location = new CodeLocation(
            Path.GetRelativePath(_rootPath, node.SyntaxTree.FilePath),
            span.StartLinePosition.Line + 1,
            span.EndLinePosition.Line + 1);
        var containingId = symbol.ContainingType?.GetDocumentationCommentId();
        _symbols.Add(new CodeSymbol(id, GetDisplayName(symbol), kind, containingId, location));
    }
    public override void VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        AddSymbol(node, CodeSymbolKind.Class);
        base.VisitClassDeclaration(node);
    }
    public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        AddSymbol(node, CodeSymbolKind.Method);
        base.VisitMethodDeclaration(node);
    }
    public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
    {
        AddSymbol(node, CodeSymbolKind.Interface);
        base.VisitInterfaceDeclaration(node);
    }
    public override void VisitStructDeclaration(StructDeclarationSyntax node)
    {
        AddSymbol(node, CodeSymbolKind.Struct);
        base.VisitStructDeclaration(node);
    }
    public override void VisitRecordDeclaration(RecordDeclarationSyntax node)
    {
        AddSymbol(node, CodeSymbolKind.Record);
        base.VisitRecordDeclaration(node);
    }
    public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
    {
        AddSymbol(node, CodeSymbolKind.Enum);
        base.VisitEnumDeclaration(node);
    }
    public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
    {
        AddSymbol(node, CodeSymbolKind.Property);
        base.VisitPropertyDeclaration(node);
    }
    public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
    {
        AddSymbol(node, CodeSymbolKind.Constructor);
        base.VisitConstructorDeclaration(node);
    }
    public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
    {
        foreach (var variable in node.Declaration.Variables)
        {
            AddSymbol(variable, CodeSymbolKind.Field);
        }
        base.VisitFieldDeclaration(node);
    }

    private static string GetDisplayName(ISymbol symbol)
    {
        return symbol is IMethodSymbol { MethodKind: MethodKind.Constructor } ? symbol.ContainingType.Name : symbol.Name;
    }
}
