using Parlot.Fluent;
using SqlParser.Core.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using static Parlot.Fluent.Parsers;

namespace SqlParser.Core.Statements
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

        public static readonly Deferred<Statement> Statement = Deferred<Statement>();

        public IEnumerable<string> ColumnNames { get; set; }

        static InsertStatement()
        {
            var number = Parser.Number
                .Then<Expression>(e => new NumericExpression(e));
            var boolean = Parser.Boolean
                .Then<Expression>(e => new BooleanExpression(Convert.ToBoolean(e)));
            var stringLiteral = Parser.StringLiteral
                .Then<Expression>(e => new LiteralExpression(e.ToString()));
            var identifier = Parser.Identifier
                .Then<Expression>(e => new IdentifierExpression(e.ToString()));
            var terminal = number.Or(boolean).Or(stringLiteral).Or(identifier);
            var columnsList = ZeroOrOne(Between(Parser.OpenParen, Separated(Parser.Comma, identifier), Parser.CloseParen));
            var valuesList = Between(Parser.OpenParen, Separated(Parser.Comma, terminal), Parser.CloseParen);
            var insertStatement = Insert.And(Into).And(identifier).And(columnsList).And(Values).And(valuesList);

            Statement.Parser = insertStatement.Then<Statement>(e =>
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

                var statement = new InsertStatement(tableName) { ColumnNames = columns };

                statement.Tokens.Add(new Token { Type = TokenType.Keyword, Value = e.Item1 });
                statement.Tokens.Add(new Token { Type = TokenType.Keyword, Value = e.Item2 });
                statement.Tokens.Add(new Token { Type = TokenType.Identifier, Value = tableName });
                statement.Tokens.Add(new Token { Type = TokenType.List, Value = columns });
                statement.Tokens.Add(new Token { Type = TokenType.Identifier, Value = e.Item5 });
                statement.Tokens.Add(new Token { Type = TokenType.List, Value = values });

                return statement;
            });
        }

        public InsertStatement(string tableName) : base(tableName)
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
