using CodeMap.Analysis.Model;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeMap.Analysis.CSharp;

internal sealed class RelationCollector : CSharpSyntaxWalker
{
    private readonly SemanticModel _model;
    private readonly string _rootPath;
    private readonly CancellationToken _cancellationToken;
    private readonly List<CodeRelation> _relations = [];

    public RelationCollector(SemanticModel model, string rootPath, CancellationToken cancellationToken = default)
    {
        _model = model;
        _rootPath = rootPath;
        _cancellationToken = cancellationToken;
    }

    public IReadOnlyList<CodeRelation> Relations => _relations;

    public override void VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        AddCall(node);
        base.VisitInvocationExpression(node);
    }
    public override void VisitSimpleBaseType(SimpleBaseTypeSyntax node)
    {
        AddBaseType(node);
        base.VisitSimpleBaseType(node);
    }
    private void AddBaseType(SimpleBaseTypeSyntax node)
    {
        var sourceId = GetSourceId(node);
        if (sourceId is null)
        {
            return;
        }

       
        var location = SyntaxLocations.ToCodeLocation(node, _rootPath);

        var target = _model.GetTypeInfo(node.Type, _cancellationToken).Type;

        // Unbound: Roslyn can't tell what this is (e.g. a NuGet package we don't have)
        if (target is null or IErrorTypeSymbol)
        {
            _relations.Add(new CodeRelation(
                sourceId, null, node.Type.ToString(),
                RelationKind.Inherits, Confidence.Unresolved, location));
            return;
        }

        // External: declared in a .NET library, not in the analyzed source
        if (!target.Locations.Any(l => l.IsInSource))
        {
            return;
        }

        var targetId = target.GetDocumentationCommentId();
        if (targetId is null)
        {
            return;
        }
        var kind = target.TypeKind == TypeKind.Interface ? RelationKind.Implements : RelationKind.Inherits;
        _relations.Add(new CodeRelation(
            sourceId, targetId,target.Name,
            kind, Confidence.Certain, location));
    }

    private void AddCall(InvocationExpressionSyntax node)
    {
        var sourceId = GetSourceId(node);
        if (sourceId is null)
        {
            return;
        }

       
        var location = SyntaxLocations.ToCodeLocation(node, _rootPath);

        var target = _model.GetSymbolInfo(node, _cancellationToken).Symbol;

        // Unbound: Roslyn can't tell what this is (e.g. a NuGet package we don't have)
        if (target is null)
        {
            _relations.Add(new CodeRelation(
                sourceId, null, node.Expression.ToString(),
                RelationKind.Calls, Confidence.Unresolved, location));
            return;
        }

        // External: declared in a .NET library, not in the analyzed source
        if (!target.Locations.Any(l => l.IsInSource))
        {
            return;
        }

        var targetId = target.GetDocumentationCommentId();
        if (targetId is null)
        {
            return;
        }

        _relations.Add(new CodeRelation(
            sourceId, targetId, $"{target.ContainingType.Name}.{target.Name}",
            RelationKind.Calls, Confidence.Certain, location));
    }

    private string? GetSourceId(SyntaxNode node)
    {
        var member = node.FirstAncestorOrSelf<MemberDeclarationSyntax>();
        if (member is null)
        {
            return null;
        }

        return _model.GetDeclaredSymbol(member, _cancellationToken)?.GetDocumentationCommentId();
    }
}
