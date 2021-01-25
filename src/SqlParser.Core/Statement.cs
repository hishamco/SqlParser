using SqlParser.Core.Syntax;
using System.Collections.Generic;

namespace SqlParser.Core
{
    public abstract class Statement
    {
        public Statement(string tableName)
        {
            TableName = tableName;
            Tokens = new List<Token>();
            Nodes = new List<SyntaxNode>();
        }

        public string TableName { get; }

        public IList<Token> Tokens { get; }

        public IList<SyntaxNode> Nodes { get; }
    }
}
