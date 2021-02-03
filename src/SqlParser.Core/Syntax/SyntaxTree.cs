using System.Collections.Generic;

namespace SqlParser.Core.Syntax
{
    public sealed class SyntaxTree
    {
        public SyntaxTree()
        {
            Statements = new List<Statement>();
        }

        public IList<Statement> Statements { get; }
    }
}
