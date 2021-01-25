using SqlParser.Core.Syntax;
using System.Collections.Generic;

namespace SqlParser.Core
{
    public abstract class Statement
    {
        public Statement(string tableName)
        {
            TableName = tableName;
            Nodes = new List<SyntaxNode>();
        }

        public string TableName { get; }

        public IList<SyntaxNode> Nodes { get; }
    }
}
