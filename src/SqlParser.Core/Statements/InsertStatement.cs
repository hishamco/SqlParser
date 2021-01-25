using Parlot.Fluent;
using SqlParser.Core.Syntax;
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
        protected static readonly Parser<string> INSERT = Terms.Text("INSERT", caseInsensitive: true);
        protected static readonly Parser<string> INTO = Terms.Text("INTO", caseInsensitive: true);
        protected static readonly Parser<string> VALUES = Terms.Text("VALUES", caseInsensitive: true);

        public static readonly Deferred<Statement> Statement = Deferred<Statement>();

        static InsertStatement()
        {
            var number = Parser.Number
                .Then(e => new SyntaxNode(new SyntaxToken
                {
                    Kind = SyntaxKind.NumberToken,
                    Value = e
                }));
            var boolean = Parser.Boolean
                .Then(e => new SyntaxNode(new SyntaxToken
                {
                    Kind = SyntaxKind.IdentifierToken,
                    Value = Convert.ToBoolean(e)
                }));
            var stringLiteral = Parser.StringLiteral
                .Then(e => new SyntaxNode(new SyntaxToken
                {
                    Kind = SyntaxKind.StringToken,
                    Value = e.ToString()
                }));
            var identifier = Parser.Identifier
                .Then(e => new SyntaxNode(new SyntaxToken
                {
                    Kind = SyntaxKind.IdentifierToken,
                    Value = e.ToString()
                }));
            var terminal = number.Or(boolean).Or(stringLiteral).Or(identifier);
            var columns = new List<string>();
            var values = new List<object>();
            var columnsList = ZeroOrOne(Between(Parser.OpenParen, Separated(Parser.Comma, identifier)
                .Then(e =>
                {
                    columns.Clear();
                    columns.AddRange(e.Select(n => n.Token.Value.ToString()));

                    return e;
                }), Parser.CloseParen)
            .Then(e =>
            {
                if (e.Count > 0)
                {
                    for (int i = 1; i < e.Count; i += 2)
                    {
                        e.Insert(i, new SyntaxNode(new SyntaxToken
                        {
                            Kind = SyntaxKind.CommaToken,
                            Value = e
                        }));
                    }

                    e.Insert(0, new SyntaxNode(new SyntaxToken
                    {
                        Kind = SyntaxKind.OpenParenthesisToken,
                        Value = Parser.OpenParen.Then(e => e)
                    }));

                    e.Add(new SyntaxNode(new SyntaxToken
                    {
                        Kind = SyntaxKind.CloseParenthesisToken,
                        Value = Parser.CloseParen.Then(e => e)
                    }));
                }

                return e;
            }));
            var valuesList = Between(Parser.OpenParen, Separated(Parser.Comma, terminal)
                .Then(e =>
                {
                    values.Clear();
                    values.AddRange(e.Select(n => n.Token.Value));

                    return e;
                }), Parser.CloseParen)
                .Then(e =>
                {
                    for (int i = 1; i < e.Count; i += 2)
                    {
                        e.Insert(i, new SyntaxNode(new SyntaxToken
                        {
                            Kind = SyntaxKind.CommaToken,
                            Value = e
                        }));
                    }

                    e.Insert(0, new SyntaxNode(new SyntaxToken
                    {
                        Kind = SyntaxKind.OpenParenthesisToken,
                        Value = Parser.OpenParen.Then(e => e)
                    }));

                    e.Add(new SyntaxNode(new SyntaxToken
                    {
                        Kind = SyntaxKind.CloseParenthesisToken,
                        Value = Parser.CloseParen.Then(e => e)
                    }));

                    return e;
                });
            var insertStatement = INSERT
                .Then(e => new SyntaxNode(new SyntaxToken
                {
                    Kind = SyntaxKind.InsertKeyword,
                    Value = e
                }))
                .And(INTO
                    .Then(e => new SyntaxNode(new SyntaxToken
                    {
                        Kind = SyntaxKind.IntoKeyword,
                        Value = e
                    })))
                .And(identifier)
                .And(columnsList)
                .And(VALUES
                    .Then(e => new SyntaxNode(new SyntaxToken
                    {
                        Kind = SyntaxKind.ValuesKeyword,
                        Value = e
                    })))
                .And(valuesList);

            Statement.Parser = insertStatement.Then<Statement>(e =>
            {
                var tableName = e.Item3.Token.Value.ToString();
                var statement = new InsertStatement(tableName)
                {
                    ColumnNames = columns,
                    Values = values
                };
                var insertIntoClause = new SyntaxNode(new SyntaxToken { Kind = SyntaxKind.InsertIntoClause });
                var valuesClause = new SyntaxNode(new SyntaxToken { Kind = SyntaxKind.ValuesClause });

                insertIntoClause.ChildNodes.Add(e.Item1);
                insertIntoClause.ChildNodes.Add(e.Item2);
                insertIntoClause.ChildNodes.Add(e.Item3);

                if (e.Item4 != null)
                {
                    foreach (var node in e.Item4)
                    {
                        insertIntoClause.ChildNodes.Add(node);
                    }
                }

                valuesClause.ChildNodes.Add(e.Item5);

                foreach (var node in e.Item6)
                {
                    valuesClause.ChildNodes.Add(node);
                }

                statement.Nodes.Add(insertIntoClause);
                statement.Nodes.Add(valuesClause);

                return statement;
            });
        }

        public InsertStatement(string tableName) : base(tableName)
        {

        }

        public IEnumerable<string> ColumnNames { get; set; }

        public IEnumerable<object> Values { get; set; }
    }
}
