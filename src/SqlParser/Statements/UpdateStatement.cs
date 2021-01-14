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
     * updateStatement ::= UPDATE tableName SET columnsAssignment
     * 
     * tableName ::= identifier
     * 
     * columnsAssignment ::= columnAssignment (, columnAssignment)*
     * 
     * columnAssignment ::= columnName = expression
     */
    public class UpdateStatement : Statement
    {
        protected static readonly Parser<char> Equal = Terms.Char('=');

        protected static readonly Parser<string> Update = Terms.Text("UPDATE", caseInsensitive: true);
        protected static readonly Parser<string> Set = Terms.Text("SET", caseInsensitive: true);

        private static readonly Deferred<IEnumerable<Token>> _tokens = Deferred<IEnumerable<Token>>();

        public string TableName { get; private set; }

        public IEnumerable<string> ColumnNames { get; set; }

        public IEnumerable<object> Values { get; set; }

        static UpdateStatement()
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
            var columnsAssignment = Separated(Parser.Comma, identifier.And(Equal).And(terminal));
            var upateStatement = Update.And(identifier).And(Set).And(columnsAssignment);
            _tokens.Parser = upateStatement.Then<IEnumerable<Token>>(e =>
            {
                var tableName = (e.Item2 as IdentifierExpression).Name;
                var columns = e.Item4.Select(a => (a.Item1 as IdentifierExpression).Name);
                var values = e.Item4.Select(a => {
                    object value = null;
                    if (a.Item3 is IdentifierExpression)
                    {
                        value = (a.Item3 as IdentifierExpression).Name;
                    }
                    else if (a.Item3 is BooleanExpression)
                    {
                        value = (a.Item3 as BooleanExpression).EvaluateAsync().GetAwaiter().GetResult().ToBooleanValue();
                    }
                    else if (a.Item3 is NumericExpression)
                    {
                        value = (a.Item3 as NumericExpression).EvaluateAsync().GetAwaiter().GetResult().ToNumberValue();
                    }
                    else if (a.Item3 is LiteralExpression)
                    {
                        value = (a.Item3 as LiteralExpression).EvaluateAsync().GetAwaiter().GetResult().ToStringValue();
                    }

                    return value;
                }).ToArray();

                return new List<Token>
                {
                    new Token { Type = TokenType.Keyword, Value = e.Item1 },
                    new Token { Type = TokenType.Identifier, Value = tableName },
                    new Token { Type = TokenType.Keyword, Value = e.Item3 },
                    new Token { Type = TokenType.List, Value = columns },
                    new Token { Type = TokenType.List, Value = values }
                };
            });
        }

        public UpdateStatement(string commandText) : base(commandText)
        {

        }

        public async override Task TokenizeAsync()
        {
            var context = new SqlParseContext(CommandText);
            var result = new ParseResult<IEnumerable<Token>>();

            _tokens.Parse(context, ref result);

            Tokens = result.Value;
            TableName = Tokens.ElementAt(1).Value.ToString();
            ColumnNames = (IEnumerable<string>)Tokens.ElementAt(3).Value;
            Values = (IEnumerable<object>)Tokens.ElementAt(4).Value;

            await Task.CompletedTask;
        }
    }
}
