using Parlot.Fluent;
using SqlParser.Core.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using static Parlot.Fluent.Parsers;

namespace SqlParser.Core.Statements
{
    /*
     * selectStatement ::= SELECT columnsList FROM tableNames
     *
     * columnsList ::= * | columnName (, columnName)*
     * 
     * columnName ::= columnName | tableName.columnName | identifier (AS alias)?
     * 
     * tablesList ::= tableName (, tableName)*
     * 
     * tableName ::= identifier (AS alias)?
     * 
     * alias ::= identifier | string
     */
    public class SelectStatement : Statement
    {
        internal protected static readonly Parser<string> Select = Terms.Text("SELECT", caseInsensitive: true);
        internal protected static readonly Parser<string> From = Terms.Text("FROM", caseInsensitive: true);
        internal protected static readonly Parser<string> As = Terms.Text("AS", caseInsensitive: true);

        public static readonly Deferred<Statement> Statement = Deferred<Statement>();

        public IEnumerable<string> ColumnNames { get; private set; }

        public IEnumerable<string> TableNames { get; private set; }

        static SelectStatement()
        {
            var identifier = Parser.Identifier
                .Then<Expression>(e => new IdentifierExpression(e.ToString()));
            var stringLiteral = Parser.StringLiteral
                .Then<Expression>(e => new LiteralExpression(e.ToString()));
            var alias = identifier.Or(stringLiteral)
                .Then(e =>
                {
                    return e.EvaluateAsync().GetAwaiter().GetResult().ToStringValue();
                });
            var column = identifier.And(ZeroOrOne(As.And(alias)))
                .Then(e => e.Item2.Item2 ?? (e.Item1 as IdentifierExpression).Name)
                .And(ZeroOrOne(Parser.Dot.And(identifier.And(ZeroOrOne(As.And(alias))))))
                .Then(e =>
                {
                    var tableName = String.Empty;
                    var columnName = e.Item1;
                    if (e.Item2.Item2.Item1 != null)
                    {
                        tableName = columnName;
                        columnName = $"{tableName}.{(e.Item2.Item2.Item1 as IdentifierExpression).Name}";
                    }

                    if (e.Item2.Item2.Item2.Item2 != null)
                    {
                        columnName = e.Item2.Item2.Item2.Item2;
                    }

                    return new IdentifierExpression(columnName) as Expression;
                });
            var columnsList = Parser.Asterisk.Then(e => new List<Expression> { new LiteralExpression(e.ToString()) })
                .Or(Separated(Parser.Comma, column));
            var tableName = identifier.And(ZeroOrOne(As.And(alias)))
                .Then(e => e.Item2.Item2 ?? (e.Item1 as IdentifierExpression).Name);
            var tablesList = Separated(Parser.Comma, tableName);
            var selectStatement = Select.And(columnsList).And(From).And(tablesList);

            Statement.Parser = selectStatement.Then<Statement>(e =>
            {
                var tableNames = e.Item4;
                var statement = new SelectStatement(tableNames.ElementAt(0))
                {
                    TableNames = tableNames
                };

                statement.Tokens.Add(new Token { Type = TokenType.Keyword, Value = e.Item1 });
                statement.Tokens.Add(new Token { Type = TokenType.Keyword, Value = e.Item3 });
                statement.Tokens.Add(new Token { Type = TokenType.List, Value = tableNames });

                if (e.Item2.Count == 1)
                {
                    if (e.Item2.First() is IdentifierExpression)
                    {
                        statement.Tokens.Insert(1, new Token { Type = TokenType.Identifier, Value = (e.Item2.First() as IdentifierExpression).Name });
                    }
                    else
                    {
                        statement.Tokens.Insert(1, new Token { Type = TokenType.Symbol, Value = '*' });
                    }
                }
                else
                {
                    statement.Tokens.Insert(1, new Token { Type = TokenType.List, Value = e.Item2.Select(i => (i as IdentifierExpression).Name) });
                }

                statement.ColumnNames = statement.Tokens.ElementAt(1).Value is IEnumerable<string>
                    ? (IEnumerable<string>)statement.Tokens.ElementAt(1).Value
                    : new List<string> { statement.Tokens.ElementAt(1).Value.ToString() };

                return statement;
            });
        }


        public SelectStatement(string tableName) : base(tableName)
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
