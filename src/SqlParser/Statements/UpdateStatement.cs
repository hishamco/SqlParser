using Parlot.Fluent;
using SqlParser.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public static readonly Deferred<Statement> Statement = Deferred<Statement>();

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
            var identifier = Parser.Identifier
                .Then<Expression>(e => new IdentifierExpression(e.ToString()));
            var terminal = number.Or(boolean).Or(stringLiteral).Or(identifier);
            var columnsAssignment = Separated(Parser.Comma, identifier.And(Equal).And(terminal));
            var upateStatement = Update.And(identifier).And(Set).And(columnsAssignment);

            Statement.Parser = upateStatement.Then<Statement>(e =>
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

                var statement = new UpdateStatement(tableName) { ColumnNames = columns, Values = values };

                statement.Tokens.Add(new Token { Type = TokenType.Keyword, Value = e.Item1 });
                statement.Tokens.Add(new Token { Type = TokenType.Identifier, Value = tableName });
                statement.Tokens.Add(new Token { Type = TokenType.Keyword, Value = e.Item3 });
                statement.Tokens.Add(new Token { Type = TokenType.List, Value = columns });
                statement.Tokens.Add(new Token { Type = TokenType.List, Value = values });

                return statement;
            });
        }


        public UpdateStatement(string tableName) : base(tableName)
        {

        }


        //public async static Task<IEnumerable<Token>> TokenizeAsync(string command)
        //{
        //    var context = new SqlContext(command);
        //    var result = new ParseResult<Statement>();

        //    Statement.Parse(context, ref result);

        //    await Task.CompletedTask;

        //    return result.Value?.Tokens ?? Enumerable.Empty<Token>();
        //}
    }
}
