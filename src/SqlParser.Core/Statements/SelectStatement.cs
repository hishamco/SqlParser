using Parlot.Fluent;
using SqlParser.Core.Syntax;
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
        private static readonly SyntaxNode EmptyNode = new SyntaxNode(new SyntaxToken());

        internal protected static readonly Parser<string> Select = Terms.Text("SELECT", caseInsensitive: true);
        internal protected static readonly Parser<string> From = Terms.Text("FROM", caseInsensitive: true);
        internal protected static readonly Parser<string> As = Terms.Text("AS", caseInsensitive: true);

        public static readonly Deferred<Statement> Statement = Deferred<Statement>();

        static SelectStatement()
        {
            var identifier = Parser.Identifier
                .Then(e => new SyntaxNode(new SyntaxToken
                {
                    Kind = SyntaxKind.IdentifierToken,
                    Value = e.ToString()
                }));
            var stringLiteral = Parser.StringLiteral
                .Then(e => new SyntaxNode(new SyntaxToken
                {
                    Kind = SyntaxKind.StringToken,
                    Value = e.ToString()
                }));
            var alias = identifier.Or(stringLiteral);
            var columns = new List<string>();
            var tables = new List<string>();
            var column = identifier
                .And(ZeroOrOne(As
                    .Then(e => new SyntaxNode(new SyntaxToken
                    {
                        Kind = SyntaxKind.AsKeyword,
                        Value = e
                    })).And(alias)))
                .And(ZeroOrOne(
                    Parser.Dot
                        .Then(e => new SyntaxNode(new SyntaxToken
                        {
                            Kind = SyntaxKind.DotToken,
                            Value = e
                        }))
                    .And(identifier
                        .And(ZeroOrOne(As
                                .Then(e => new SyntaxNode(new SyntaxToken
                                {
                                    Kind = SyntaxKind.AsKeyword,
                                    Value = e
                                }))
                            .And(alias))))))
                .Then(e =>
                {
                    columns.Clear();

                    return e;
                });
            var columnsList = Parser.Asterisk
                .Then(e =>
                {
                    columns.Clear();

                    return new List<(SyntaxNode, (SyntaxNode, SyntaxNode), (SyntaxNode, (SyntaxNode, (SyntaxNode, SyntaxNode))))>
                    {
                        (new SyntaxNode(new SyntaxToken
                        {
                            Kind = SyntaxKind.AsteriskToken,
                            Value = e
                        }),
                        (EmptyNode, EmptyNode),
                        (EmptyNode, (EmptyNode, (EmptyNode, EmptyNode))))
                    };
                })
                .Or(Separated(Parser.Comma, column))
                .Then(e =>
                {
                    for (int i = 1; i < e.Count; i += 2)
                    {
                        e.Insert(i, (new SyntaxNode(new SyntaxToken
                        {
                            Kind = SyntaxKind.CommaToken,
                            Value = ","
                        }),
                        (EmptyNode, EmptyNode),
                        (EmptyNode, (EmptyNode, (EmptyNode, EmptyNode)))));
                    }

                    for (int i = 0; i < e.Count; i += 2)
                    {
                        var columnName = e[i].Item1.Token.Value.ToString();
                        if (e[i].Item2.Item2 != null && e[i].Item2.Item2 != EmptyNode)
                        {
                            columnName = e[i].Item2.Item2.Token.Value.ToString();
                        }

                        if (e[i].Item3.Item2.Item1 != null && e[i].Item3.Item2.Item1 != EmptyNode)
                        {
                            columnName = e[i].Item3.Item2.Item1.Token.Value.ToString();
                        }

                        if (e[i].Item3.Item2.Item2.Item2 != null && e[i].Item3.Item2.Item2.Item2 != EmptyNode)
                        {
                            columnName = e[i].Item3.Item2.Item2.Item2.Token.Value.ToString();
                        }

                        columns.Add(columnName);
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
                    var nodes = new List<SyntaxNode>();
                    var tableName = e.Item1.Token.Value.ToString();

                    nodes.Add(new SyntaxNode(new SyntaxToken
                    {
                        Kind = SyntaxKind.IdentifierToken,
                        Value = tableName
                    }));

                    if (e.Item2.Item2 != null)
                    {
                        tableName = e.Item2.Item2.Token.Value.ToString();

                        nodes.Add(new SyntaxNode(new SyntaxToken
                        {
                            Kind = SyntaxKind.AsKeyword,
                            Value = e.Item2.Item1.Token.Value
                        }));

                        if (e.Item2.Item2.Token.Kind == SyntaxKind.StringToken)
                        {
                            nodes.Add(new SyntaxNode(new SyntaxToken
                            {
                                Kind = SyntaxKind.StringToken,
                                Value = tableName
                            }));
                        }
                        else
                        {
                            nodes.Add(new SyntaxNode(new SyntaxToken
                            {
                                Kind = SyntaxKind.IdentifierToken,
                                Value = tableName
                            }));
                        }
                    }

                    return nodes;
                });
            var tablesList = Separated(Parser.Comma, table)
                .Then(e =>
                {
                    tables.Clear();

                    foreach (var node in e)
                    {
                        if (node.Count == 3)
                        {
                            tables.Add(node[2].Token.Value.ToString());
                        }
                        else
                        {
                            tables.Add(node[0].Token.Value.ToString());
                        }

                        node.Add(new SyntaxNode(new SyntaxToken
                        {
                            Kind = SyntaxKind.CommaToken,
                            Value = ","
                        }));
                    }

                    var lastNode = e[e.Count - 1];
                    lastNode.RemoveAt(lastNode.Count - 1);

                    return e.SelectMany(n => n);
                });
            var selectStatement = Select
                .Then(e => new SyntaxNode(new SyntaxToken
                {
                    Kind = SyntaxKind.SelectKeyword,
                    Value = e
                })).And(columnsList).And(From
                        .Then(e => new SyntaxNode(new SyntaxToken
                        {
                            Kind = SyntaxKind.FromKeyword,
                            Value = e
                        })))
                    .And(tablesList);

            Statement.Parser = selectStatement.Then<Statement>(e =>
            {
                var statement = new SelectStatement(tables[0])
                {
                    TableNames = tables,
                    ColumnNames = columns
                };
                var selectClause = new SyntaxNode(new SyntaxToken { Kind = SyntaxKind.SelectClause });
                var fromClause = new SyntaxNode(new SyntaxToken { Kind = SyntaxKind.FromClause });

                selectClause.ChildNodes.Add(e.Item1);

                for (int i = 0; i < e.Item2.Count; i++)
                {
                    if (e.Item2.Count == 1 && e.Item2[0].Item1.Token.Kind == SyntaxKind.AsteriskToken)
                    {
                        selectClause.ChildNodes.Add(e.Item2[0].Item1);
                        break;
                    }
                    else
                    {
                        selectClause.ChildNodes.Add(e.Item2[i].Item1);

                        if (e.Item2[i].Item2.Item1 != null && e.Item2[i].Item2.Item1 != EmptyNode)
                        {
                            selectClause.ChildNodes.Add(e.Item2[i].Item2.Item1);
                            selectClause.ChildNodes.Add(e.Item2[i].Item2.Item2);
                        }

                        if (e.Item2[i].Item3.Item1 != null && e.Item2[i].Item3.Item1 != EmptyNode)
                        {
                            selectClause.ChildNodes.Add(e.Item2[i].Item3.Item1);
                            selectClause.ChildNodes.Add(e.Item2[i].Item3.Item2.Item1);
                        }

                        if (e.Item2[i].Item3.Item2.Item2.Item1 != null && e.Item2[i].Item3.Item2.Item2.Item1 != EmptyNode)
                        {
                            selectClause.ChildNodes.Add(e.Item2[i].Item3.Item2.Item2.Item1);
                            selectClause.ChildNodes.Add(e.Item2[i].Item3.Item2.Item2.Item2);
                        }
                    }
                }

                fromClause.ChildNodes.Add(e.Item3);

                foreach (var node in e.Item4)
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

        public IEnumerable<string> ColumnNames { get; private set; }

        public IEnumerable<string> TableNames { get; private set; }
    }
}
