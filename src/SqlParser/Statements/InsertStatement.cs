using Parlot;
using Parlot.Fluent;
using SqlParser.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Parlot.Fluent.Parsers;

namespace SqlParser.Statements
{
    /*
     * insertStatement ::= INSERT INTO tableName (columnsList)? VALUES (valuesList)
     * 
     * tableName :: = identifier
     * 
     * columnsList :: identifier (, identifier)*
     * 
     * valuesList :: expression (, expression)*
     */
    public class InsertStatement : Statement
    {
        protected static readonly Parser<string> Insert = Terms.Text("INSERT", caseInsensitive: true);
        protected static readonly Parser<string> Into = Terms.Text("INTO", caseInsensitive: true);
        protected static readonly Parser<string> Values = Terms.Text("VALUES", caseInsensitive: true);

        private static readonly Deferred<IEnumerable<Token>> _tokens = Deferred<IEnumerable<Token>>();

        public string TableName { get; private set; }

        public IEnumerable<string> ColumnNames { get; set; }

        static InsertStatement()
        {
            var number = Terms.Decimal(NumberOptions.AllowSign)
                .Then<Expression>(e => new NumericExpression(e));
            var boolean = Parser.True.Or(Parser.False)
                .Then<Expression>(e => new BooleanExpression(Convert.ToBoolean(e)));
            var stringLiteral = Terms.String(StringLiteralQuotes.SingleOrDouble)
                .Then<Expression>(e => new LiteralExpression(e.ToString()));
            var identifier = Terms.Identifier()
                .Then<Expression>(e => new IdentifierExpression(e.ToString()));
            var terminal = number.Or(boolean).Or(stringLiteral).Or(identifier);
            var columnsList = ZeroOrOne(Between(Parser.OpenParen, Separated(Parser.Comma, identifier), Parser.CloseParen));
            var valuesList = Between(Parser.OpenParen, Separated(Parser.Comma, terminal), Parser.CloseParen);
            var insertStatement = Insert.And(Into).And(identifier).And(columnsList).And(Values).And(valuesList);
            _tokens.Parser = insertStatement.Then<IEnumerable<Token>>(e =>
            {
                var tableName = (e.Item3 as IdentifierExpression).Name;
                var columns = e.Item4 == null
                    ? Enumerable.Empty<string>()
                    : e.Item4.Select(i => (i as IdentifierExpression).Name);
                var values = e.Item6.Select(v => {
                    object value = null;
                    if (v is IdentifierExpression)
                    {
                        value = (v as IdentifierExpression).Name;
                    }
                    else if (v is BooleanExpression)
                    {
                        value = (v as BooleanExpression).EvaluateAsync().GetAwaiter().GetResult().ToBooleanValue();
                    }
                    else if (v is NumericExpression)
                    {
                        value = (v as NumericExpression).EvaluateAsync().GetAwaiter().GetResult().ToNumberValue();
                    }
                    else if (v is LiteralExpression)
                    {
                        value = (v as LiteralExpression).EvaluateAsync().GetAwaiter().GetResult().ToStringValue();
                    }

                    return value;
                }).ToArray();

                return new List<Token>
                {
                    new Token { Type = TokenType.Keyword, Value = e.Item1 },
                    new Token { Type = TokenType.Keyword, Value = e.Item2 },
                    new Token { Type = TokenType.Identifier, Value = tableName },
                    new Token { Type = TokenType.List, Value = columns },
                    new Token { Type = TokenType.Identifier, Value = e.Item5 },
                    new Token { Type = TokenType.List, Value = values }
                };
            });
        }

        public InsertStatement(string commandText) : base(commandText)
        {

        }

        public async override Task TokenizeAsync()
        {
            var context = new SqlParseContext(CommandText);
            var result = new ParseResult<IEnumerable<Token>>();

            _tokens.Parse(context, ref result);

            Tokens = result.Value;
            TableName = Tokens.ElementAt(2).Value.ToString();
            ColumnNames = (IEnumerable<string>)Tokens.ElementAt(3).Value;

            await Task.CompletedTask;
        }
    }
}
