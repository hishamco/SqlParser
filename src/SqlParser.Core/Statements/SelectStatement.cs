using Parlot.Fluent;
using SqlParser.Core.Syntax;
using System.Collections.Generic;
using System.Linq;
using static Parlot.Fluent.Parsers;

namespace SqlParser.Core.Statements
{
    /*
     * selectStatement ::= SELECT DISTINCT? topExpression? columnsList FROM tableNames
     * 
     * topExpression ::= TOP(number)
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
        internal protected static readonly Parser<string> Distinct = Terms.Text("DISTINCT", caseInsensitive: true);
        internal protected static readonly Parser<string> Top = Terms.Text("TOP", caseInsensitive: true);
        internal protected static readonly Parser<string> From = Terms.Text("FROM", caseInsensitive: true);
        internal protected static readonly Parser<string> As = Terms.Text("AS", caseInsensitive: true);

        public static readonly Deferred<Statement> Statement = Deferred<Statement>();

        static SelectStatement()
        {
            var number = SqlParser.Number
               .Then(e => new SyntaxNode(new SyntaxToken
               {
                   Kind = SyntaxKind.NumberToken,
                   Value = e
               }));
            var identifier = SqlParser.Identifier
                .Then(e => new SyntaxNode(new SyntaxToken
                {
                    Kind = SyntaxKind.IdentifierToken,
                    Value = e.ToString()
                }));
            var stringLiteral = SqlParser.StringLiteral
                .Then(e => new SyntaxNode(new SyntaxToken
                {
                    Kind = SyntaxKind.StringToken,
                    Value = e.ToString()
                }));
            var alias = identifier.Or(stringLiteral);
            SyntaxNode columnNode = null;
            var column = identifier
                .And(ZeroOrOne(As.And(alias))).Then(e =>
                    {
                        columnNode = new SyntaxNode(new SyntaxToken());

                        columnNode.ChildNodes.Add(e.Item1);

                        if (e.Item2.Item1 != null)
                        {
                            columnNode.ChildNodes.Add(new SyntaxNode(new SyntaxToken
                            {
                                Kind = SyntaxKind.AsKeyword,
                                Value = e.Item2.Item1
                            }));
                            columnNode.ChildNodes.Add(e.Item2.Item2);
                        }

                        return columnNode;
                    })
                .And(ZeroOrOne(
                    SqlParser.Dot
                    .And(identifier
                        .And(ZeroOrOne(
                            As.And(alias)))))
                .Then(e =>
                {
                    if (e.Item2.Item1 != null)
                    {
                        columnNode.ChildNodes.Add(new SyntaxNode(new SyntaxToken
                        {
                            Kind = SyntaxKind.DotToken,
                            Value = e.Item1
                        }));
                        columnNode.ChildNodes.Add(e.Item2.Item1);
                    }

                    if (e.Item2.Item2.Item1 != null)
                    {
                        columnNode.ChildNodes.Add(new SyntaxNode(new SyntaxToken
                        {
                            Kind = SyntaxKind.AsKeyword,
                            Value = e.Item2.Item2.Item1
                        }));
                        columnNode.ChildNodes.Add(e.Item2.Item2.Item2);
                    }

                    return columnNode;
                })).Then(e => columnNode);
            var columnsList = SqlParser.Asterisk
                .Then(e =>
                {
                    columnNode = new SyntaxNode(new SyntaxToken());
                    columnNode.ChildNodes.Add(new SyntaxNode(new SyntaxToken
                    {
                        Kind = SyntaxKind.AsteriskToken,
                        Value = e
                    }));

                    return new List<SyntaxNode> { columnNode };
                })
                .Or(Separated(SqlParser.Comma, column))
                .Then(e =>
                {
                    for (int i = 1; i < e.Count; i += 2)
                    {
                        e.Insert(i, (new SyntaxNode(new SyntaxToken
                        {
                            Kind = SyntaxKind.CommaToken,
                            Value = ","
                        })));
                    }

                    return e;
                });
            var table = identifier.And(ZeroOrOne(As
                .Then(e => new SyntaxNode(new SyntaxToken
                {
                    Kind = SyntaxKind.AsKeyword,
                    Value = e
                })).And(alias)))
                .Then(e =>
                {
                    var tableNode = new SyntaxNode(new SyntaxToken());
                    var tableName = e.Item1.Token.Value.ToString();

                    tableNode.ChildNodes.Add(new SyntaxNode(new SyntaxToken
                    {
                        Kind = SyntaxKind.IdentifierToken,
                        Value = tableName
                    }));

                    if (e.Item2.Item2 != null)
                    {
                        tableName = e.Item2.Item2.Token.Value.ToString();

                        tableNode.ChildNodes.Add(new SyntaxNode(new SyntaxToken
                        {
                            Kind = SyntaxKind.AsKeyword,
                            Value = e.Item2.Item1.Token.Value
                        }));

                        if (e.Item2.Item2.Token.Kind == SyntaxKind.StringToken)
                        {
                            tableNode.ChildNodes.Add(new SyntaxNode(new SyntaxToken
                            {
                                Kind = SyntaxKind.StringToken,
                                Value = tableName
                            }));
                        }
                        else
                        {
                            tableNode.ChildNodes.Add(new SyntaxNode(new SyntaxToken
                            {
                                Kind = SyntaxKind.IdentifierToken,
                                Value = tableName
                            }));
                        }
                    }

                    return tableNode;
                });
            var tablesList = Separated(SqlParser.Comma, table)
                .Then(e =>
                {
                    for (int i = 1; i < e.Count; i += 2)
                    {
                        e.Insert(i, (new SyntaxNode(new SyntaxToken
                        {
                            Kind = SyntaxKind.CommaToken,
                            Value = ","
                        })));
                    }

                    return e;
                });
            var topExpression = ZeroOrOne(Top
                .Then(e => new SyntaxNode(new SyntaxToken
                {
                    Kind = SyntaxKind.TopKeyword,
                    Value = e
                }))
                .And(Between(SqlParser.OpenParen, number, SqlParser.CloseParen)))
                .Then(e => new List<SyntaxNode>
                    {
                        e.Item1,
                        new SyntaxNode(new SyntaxToken
                        {
                            Kind = SyntaxKind.OpenParenthesisToken,
                            Value = '('
                        }),
                        e.Item2,
                        new SyntaxNode(new SyntaxToken
                        {
                            Kind = SyntaxKind.CloseParenthesisToken,
                            Value = ')'
                        })
                    });
            var selectStatement = Select
                .Then(e => new SyntaxNode(new SyntaxToken
                {
                    Kind = SyntaxKind.SelectKeyword,
                    Value = e
                }))
                .And(ZeroOrOne(Distinct
                    .Then(e => new SyntaxNode(new SyntaxToken
                    {
                        Kind = SyntaxKind.DistinctKeyword,
                        Value = e
                    }))))
                .And(topExpression)
                .And(columnsList)
                .And(From
                    .Then(e => new SyntaxNode(new SyntaxToken
                    {
                        Kind = SyntaxKind.FromKeyword,
                        Value = e
                    })))
                .And(tablesList);

            Statement.Parser = selectStatement.Then<Statement>(e =>
            {
                var tableNames = e.Item6
                    .Where(n => n.Token.Kind != SyntaxKind.CommaToken)
                    .Select(e => e.ChildNodes[0].Token.Value.ToString())
                    .ToList();
                var tableAliases = e.Item6
                    .Where(n => n.Token.Kind != SyntaxKind.CommaToken && n.ChildNodes.Any(c => c.Kind == SyntaxKind.AsKeyword))
                    .Select(e => e.ChildNodes[e.ChildNodes.Count - 1].Token.Value.ToString())
                    .ToList();
                var columnNames = e.Item4
                    .Where(n => n.Token.Kind != SyntaxKind.CommaToken)
                    .Select(e => e.ChildNodes[0].Token.Kind == SyntaxKind.AsteriskToken
                        ? e.ChildNodes[0].Token.Value.ToString()
                        : e.ChildNodes.Any(n => n.Kind == SyntaxKind.DotToken)
                            ? e.ChildNodes[2].Token.Value.ToString()
                            : e.ChildNodes[0].Token.Value.ToString())
                    .ToList();
                var columnAliases = e.Item4
                    .Where(n => n.Token.Kind != SyntaxKind.CommaToken && n.ChildNodes.Any(c => c.Kind == SyntaxKind.AsKeyword))
                    .Select(e => e.ChildNodes[e.ChildNodes.Count - 1].Token.Value.ToString())
                    .ToList();
                var statement = new SelectStatement(tableNames[0])
                {
                    TableAliases = tableAliases,
                    TableNames = tableNames,
                    ColumnAliases = columnAliases,
                    ColumnNames = columnNames
                };
                var selectClause = new SyntaxNode(new SyntaxToken { Kind = SyntaxKind.SelectClause });
                var fromClause = new SyntaxNode(new SyntaxToken { Kind = SyntaxKind.FromClause });

                selectClause.ChildNodes.Add(e.Item1);

                if (e.Item2 != null)
                {
                    selectClause.ChildNodes.Add(e.Item2);
                }

                if (e.Item3[0] != null)
                {
                    foreach (var node in e.Item3)
                    {
                        selectClause.ChildNodes.Add(node);
                    }
                }

                foreach (var node in e.Item4)
                {
                    selectClause.ChildNodes.Add(node);
                }

                fromClause.ChildNodes.Add(e.Item5);

                foreach (var node in e.Item6)
                {
                    fromClause.ChildNodes.Add(node);
                }

                statement.Nodes.Add(selectClause);
                statement.Nodes.Add(fromClause);

                return statement;
            });
        }

        public SelectStatement(string tableName) : base(tableName)
        {

        }

        public IEnumerable<string> ColumnAliases { get; private set; }

        public IEnumerable<string> ColumnNames { get; private set; }

        public IEnumerable<string> TableAliases { get; private set; }

        public IEnumerable<string> TableNames { get; private set; }
    }
}
