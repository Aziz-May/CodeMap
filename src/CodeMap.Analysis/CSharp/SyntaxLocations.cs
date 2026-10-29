using CodeMap.Analysis.Model;

using Microsoft.CodeAnalysis;

namespace CodeMap.Analysis.CSharp;


internal static class SyntaxLocations
{
    public static CodeLocation ToCodeLocation(SyntaxNode node, string rootPath)
    {
        var span = node.GetLocation().GetLineSpan();
        return new CodeLocation(
            Path.GetRelativePath(rootPath, node.SyntaxTree.FilePath),
            span.StartLinePosition.Line + 1,
            span.EndLinePosition.Line + 1);
    }
}
