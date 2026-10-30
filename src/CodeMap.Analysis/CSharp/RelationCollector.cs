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

    public override void VisitParameter(ParameterSyntax node)
    {
        AddInjection(node);
        base.VisitParameter(node);
    }
    public override void VisitSimpleBaseType(SimpleBaseTypeSyntax node)
    {
        AddBaseType(node);
        base.VisitSimpleBaseType(node);
    }
    public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
    {
        AddInstantiation(node);
        base.VisitObjectCreationExpression(node);
    }
    public override void VisitImplicitObjectCreationExpression(ImplicitObjectCreationExpressionSyntax node)
    {
        AddInstantiation(node);
        base.VisitImplicitObjectCreationExpression(node);
    }

    private void AddInjection(ParameterSyntax node)
    {
        if(node.Parent?.Parent is not ConstructorDeclarationSyntax)
        {
            return;
        }
        if (node.Type is null)
        {
            return;
        }
        var target = _model.GetTypeInfo(node.Type , _cancellationToken).Type;
        AddRelation(node, target, node.Type.ToString(), RelationKind.Injects);
    }
    private void AddInstantiation(BaseObjectCreationExpressionSyntax node)
    {
        var target = _model.GetTypeInfo(node, _cancellationToken).Type;
        var unresolvedName = node is ObjectCreationExpressionSyntax creation
            ? creation.Type.ToString()
            : "new()";
        AddRelation(node, target, unresolvedName, RelationKind.Instantiates);
    }
    private void AddBaseType(SimpleBaseTypeSyntax node)
    {
        var target = _model.GetTypeInfo(node.Type, _cancellationToken).Type;
        var kind = target is { TypeKind: TypeKind.Interface } ? RelationKind.Implements : RelationKind.Inherits;

        AddRelation(node, target, node.Type.ToString(), kind);
    }

    private void AddCall(InvocationExpressionSyntax node)
    {
        var target = _model.GetSymbolInfo(node, _cancellationToken).Symbol;

        AddRelation(node, target, node.Expression.ToString(), RelationKind.Calls);
    }

    private void AddRelation(SyntaxNode node, ISymbol? target, string unresolvedName, RelationKind kind)
    {
        var sourceId = GetSourceId(node);
        if (sourceId is null)
        {
            return;
        }

        var location = SyntaxLocations.ToCodeLocation(node, _rootPath);

        // Unbound: Roslyn can't tell what this is (e.g. a NuGet package we don't have)
        if (target is null or IErrorTypeSymbol)
        {
            _relations.Add(new CodeRelation(
                sourceId, null, unresolvedName,
                kind, Confidence.Unresolved, location));
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
            sourceId, targetId, GetDisplayName(target),
            kind, Confidence.Certain, location));
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

    private static string GetDisplayName(ISymbol symbol) =>
        symbol.ContainingType is null ? symbol.Name : $"{symbol.ContainingType.Name}.{symbol.Name}";
}
