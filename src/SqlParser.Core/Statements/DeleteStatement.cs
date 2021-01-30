using Parlot.Fluent;
using SqlParser.Core.Syntax;
using static Parlot.Fluent.Parsers;

namespace SqlParser.Core.Statements
{
    /*
     * deleteStatement ::= DELETE FROM tableName
     * 
     * tableName ::= identifier
     */
    public class DeleteStatement : Statement
    {
        private static readonly Parser<string> Delete = Terms.Text("DELETE", caseInsensitive: true);
        private static readonly Parser<string> From = Terms.Text("FROM", caseInsensitive: true);

        public static readonly Deferred<Statement> Statement = Deferred<Statement>();

        static DeleteStatement()
        {
            var identifier = SqlParser.Identifier
                .Then(e => new SyntaxNode(new SyntaxToken
                {
                    Kind = SyntaxKind.IdentifierToken,
                    Value = e.ToString()
                }));
            var deleteStatement = Delete
                .Then(e => new SyntaxNode(new SyntaxToken
                {
                    Kind = SyntaxKind.DeleteKeyword,
                    Value = e
                }))
                .And(From
                        .Then(e => new SyntaxNode(new SyntaxToken
                        {
                            Kind = SyntaxKind.FromKeyword,
                            Value = e
                        })))
                .And(identifier);

            Statement.Parser = deleteStatement.Then<Statement>(e =>
            {
                var tableName = e.Item3.Token.Value.ToString();
                var statement = new DeleteStatement(tableName);
                var deleteClause = new SyntaxNode(new SyntaxToken { Kind = SyntaxKind.DeleteClause });
                var fromClause = new SyntaxNode(new SyntaxToken { Kind = SyntaxKind.FromClause });

                deleteClause.ChildNodes.Add(e.Item1);
                fromClause.ChildNodes.Add(e.Item2);
                fromClause.ChildNodes.Add(e.Item3);

                statement.Nodes.Add(deleteClause);
                statement.Nodes.Add(fromClause);

                return statement;
            });
        }

        public DeleteStatement(string tableName) : base(tableName)
        {

        }
    }
}
