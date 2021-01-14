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
     * selectStatement ::= SELECT columnsList FROM tableName
     *
     * columnsList :: * | identifier (, identifier)*
     * 
     * tableName :: = identifier
     */
    public class SelectStatement : Statement
    {
        protected static readonly Parser<char> OpenParen = Terms.Char('(');
        protected static readonly Parser<char> CloseParen = Terms.Char(')');
        protected static readonly Parser<char> Comma = Terms.Char(',');
        protected static readonly Parser<char> Asterisk = Terms.Char('*');

        protected static readonly Parser<string> Select = Terms.Text("SELECT", caseInsensitive: true);
        protected static readonly Parser<string> From = Terms.Text("FROM", caseInsensitive: true);

        private static readonly Deferred<IEnumerable<Token>> _tokens = Deferred<IEnumerable<Token>>();

        public string TableName { get; private set; }

        public IEnumerable<string> ColumnNames { get; set; }

        static SelectStatement()
        {
            var identifier = Terms.Identifier()
                .Then<Expression>(e => new IdentifierExpression(e.ToString()));
            var columnsList = Asterisk.Then(e => new List<Expression> { new LiteralExpression(e.ToString()) })
                .Or(Separated(Comma, identifier));
            var selectStatement = Select.And(columnsList).And(From).And(identifier);
            _tokens.Parser = selectStatement.Then<IEnumerable<Token>>(e =>
            {
                var tableName = (e.Item4 as IdentifierExpression).Name;
                var tokens = new List<Token>
                {
                    new Token { Type = TokenType.Keyword, Value = e.Item1 },
                    new Token { Type = TokenType.Keyword, Value = e.Item3 },
                    new Token { Type = TokenType.Identifier, Value = tableName }
                };
                if (e.Item2.Count == 1)
                {
                    if (e.Item2.First() is IdentifierExpression)
                    {
                        tokens.Insert(1, new Token { Type = TokenType.Identifier, Value = (e.Item2.First() as IdentifierExpression).Name });
                    }
                    else
                    {
                        tokens.Insert(1, new Token { Type = TokenType.Symbol, Value = '*' });
                    }
                }
                else
                {
                    var columns = e.Item2.Select(i => (i as IdentifierExpression).Name);
                    tokens.Insert(1, new Token { Type = TokenType.List, Value = columns });
                }

                return tokens;
            });
        }

        public SelectStatement(string commandText) : base(commandText)
        {

        }

        public async override Task TokenizeAsync()
        {
            var context = new SqlParseContext(CommandText);
            var result = new ParseResult<IEnumerable<Token>>();

            _tokens.Parse(context, ref result);

            Tokens = result.Value;
            TableName = Tokens.Last().Value.ToString();

            if (Tokens.ElementAt(1).Value is IEnumerable<string>)
            {
                ColumnNames = (IEnumerable<string>)Tokens.ElementAt(1).Value;
            }
            else
            {
                ColumnNames = new List<string> { Tokens.ElementAt(1).Value.ToString() };
            }

            await Task.CompletedTask;
        }
    }
}
