using Parlot.Fluent;
using SqlParser.Core.Expressions;
using static Parlot.Fluent.Parsers;

namespace SqlParser.Core.Statements
{
    /*
     * deleteStatement ::= DELETE FROM tableName
     * 
     * tableName :: = identifier
     */
    public class DeleteStatement : Statement
    {
        private static readonly Parser<string> Delete = Terms.Text("DELETE", caseInsensitive: true);
        private static readonly Parser<string> From = Terms.Text("FROM", caseInsensitive: true);

        public static readonly Deferred<Statement> Statement = Deferred<Statement>();

        static DeleteStatement()
        {
            var identifier = Parser.Identifier
                .Then<Expression>(e => new IdentifierExpression(e.ToString()));
            var deleteStatement = Delete.And(From).And(identifier);

            Statement.Parser = deleteStatement.Then<Statement>(e =>
            {
                var tableName = (e.Item3 as IdentifierExpression).Name;
                var statement = new DeleteStatement(tableName);

                statement.Tokens.Add(new Token { Type = TokenType.Keyword, Value = e.Item1 });
                statement.Tokens.Add(new Token { Type = TokenType.Keyword, Value = e.Item2 });
                statement.Tokens.Add(new Token { Type = TokenType.Identifier, Value = tableName });

                return statement;
            });
        }

        public DeleteStatement(string tableName) : base(tableName)
        {

        }

        //public async Task<IEnumerable<Token>> TokenizeAsync(string command)
        //{
        //    var context = new SqlContext(command);
        //    var result = new ParseResult<Statement>();

        //    Statement.Parse(context, ref result);

        //    await Task.CompletedTask;

        //    return result.Value?.Tokens ?? Enumerable.Empty<Token>();
        //}
    }
}
