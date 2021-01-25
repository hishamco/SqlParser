using System.Collections.Generic;

namespace SqlParser.Core.Syntax
{
    public class SyntaxNode
    {
        public SyntaxNode(SyntaxToken token)
        {
            Token = token;
            ChildNodes = new List<SyntaxNode>();
        }

        public SyntaxKind Kind => Token.Kind;

        public SyntaxToken Token { get; }

        public IList<SyntaxNode> ChildNodes { get; }
    }
}
