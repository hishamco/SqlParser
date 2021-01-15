using Parlot;
using Parlot.Fluent;
using SqlParser.Expressions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Parlot.Fluent.Parsers;

namespace SqlParser.Statements
{
    /*
     * deleteStatement ::= DELETE FROM tableName
     * 
     * tableName :: = identifier
     */
    public class DeleteStatement : Statement
    {
        protected static readonly Parser<string> Delete = Terms.Text("DELETE", caseInsensitive: true);
        protected static readonly Parser<string> From = Terms.Text("FROM", caseInsensitive: true);

        private static readonly Deferred<IEnumerable<Token>> _tokens = Deferred<IEnumerable<Token>>();

        public string TableName { get; private set; }

        static DeleteStatement()
        {
            var identifier = Terms.Identifier()
                .Then<Expression>(e => new IdentifierExpression(e.ToString()));
            var deleteStatement = Delete.And(From).And(identifier);
            _tokens.Parser = deleteStatement.Then<IEnumerable<Token>>(e =>
            {
                var tableName = (e.Item3 as IdentifierExpression).Name;

                return new List<Token>
                {
                    new Token { Type = TokenType.Keyword, Value = e.Item1 },
                    new Token { Type = TokenType.Keyword, Value = e.Item2 },
                    new Token { Type = TokenType.Identifier, Value = tableName }
                };
            });
        }

        public DeleteStatement(string commandText) : base(commandText)
        {

        }

        public async override Task TokenizeAsync()
        {
            var context = new SqlContext(CommandText);
            var result = new ParseResult<IEnumerable<Token>>();

            _tokens.Parse(context, ref result);

            Tokens = result.Value;
            TableName = Tokens.Last().Value.ToString();

            await Task.CompletedTask;
        }
    }
}
