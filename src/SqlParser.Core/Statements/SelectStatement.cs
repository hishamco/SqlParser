using Parlot.Fluent;
using SqlParser.Core.Syntax;
using System.Collections.Generic;
using System.Linq;
using static Parlot.Fluent.Parsers;

namespace SqlParser.Core.Statements
{
    /*
     * selectStatement ::= SELECT DISTINCT? topExpression? columnsList FROM tableNames (whereClause)? (orderByClause)? | SELECT valuesList (whereClause)? (orderByClause)?
     * 
     * topExpression ::= TOP(number)
     *
     * columnsList ::= * | columnName (, columnName)*
     * 
     * columnName ::= ((identifier '.')? identifier | functionExpression) (AS alias)?
     * 
     * valuesList ::= value (, value)*
     * 
     * value ::= (expression | functionExpression) (AS alias)?
     * 
     * functionExpression ::= identifier '(' ('*' | functionParameters) ')'
     * 
     * functionParameters :: functionParameter (, functionParameter)*
     * 
     * functionParameter :: expression | (identifier '.')? identifier
     * 
     * tablesList ::= tableName (, tableName)*
     * 
     * tableName ::= identifier (AS alias)?
     * 
     * alias ::= identifier | string
     * 
     * whereClause ::= WHERE (comparisonExpression | logicalExpression | bitwiseExpression | functionExpression)
     * 
     * logicalExpression ::= comparisonExpression AND comparisonExpression | comparisonExpression OR comparisonExpression | NOT comparisonExpression
     * 
     * comparisonExpression ::= expression = expression | expression <> expression | expression != expression
     * 
     *                          expression < expression | expression > expression | expression <= expression | expression >= expression | boolean
     * 
     * bitwiseExpression ::= ~ expression
     * orderByClause ::= ORDER BY columnName (, columnName)* (ASC | DESC)
     */
    public class SelectStatement : Statement
    {
        internal protected static readonly Parser<string> Select = Terms.Text("SELECT", caseInsensitive: true);
        internal protected static readonly Parser<string> Distinct = Terms.Text("DISTINCT", caseInsensitive: true);
        internal protected static readonly Parser<string> Top = Terms.Text("TOP", caseInsensitive: true);
        internal protected static readonly Parser<string> From = Terms.Text("FROM", caseInsensitive: true);
        internal protected static readonly Parser<string> As = Terms.Text("AS", caseInsensitive: true);
        internal protected static readonly Parser<string> Where = Terms.Text("WHERE", caseInsensitive: true);
        internal protected static readonly Parser<string> OrderBy = Terms.Text("ORDER BY", caseInsensitive: true);
        internal protected static readonly Parser<string> Ascending = Terms.Text("ASC", caseInsensitive: true);
        internal protected static readonly Parser<string> Descending = Terms.Text("DESC", caseInsensitive: true);
        internal protected static readonly Parser<string> And = Terms.Text("AND", caseInsensitive: true);
        internal protected static readonly Parser<string> Or = Terms.Text("OR", caseInsensitive: true);
        internal protected static readonly Parser<string> Not = Terms.Text("NOT", caseInsensitive: true);

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
            var expression = new SqlParser().Expression;
            var functionParameter = ZeroOrOne(identifier.And(SqlParser.Dot))
                .And(identifier
                    .And(ZeroOrOne(As.And(alias))))
                .Then(e =>
                {
                    var paramNode = new SyntaxNode(new SyntaxToken
                    {
                        Kind = SyntaxKind.IdentifierToken,
                        Value = e.Item3.Item1.Token.Value
                    });
                    var prevParamNode = paramNode;
                    if (e.Item1 != null)
                    {
                        paramNode = new SyntaxNode(new SyntaxToken
                        {
                            Kind = SyntaxKind.DotToken,
                            Value = e.Item2
                        });

                        paramNode.ChildNodes.Add(e.Item1);
                        paramNode.ChildNodes.Add(prevParamNode);
                    }

                    if (e.Item3.Item2.Item1 != null)
                    {
                        paramNode = new SyntaxNode(new SyntaxToken
                        {
                            Kind = SyntaxKind.AsKeyword,
                            Value = e.Item3.Item2.Item1
                        });

                        paramNode.ChildNodes.Add(prevParamNode);
                        paramNode.ChildNodes.Add(e.Item3.Item2.Item2);
                    }

                    return paramNode;
                })
                .Or(expression);
            var functionParameters = SqlParser.Asterisk
                .Then(e => new List<SyntaxNode>
                {
                    new SyntaxNode(new SyntaxToken
                    {
                        Kind = SyntaxKind.AsteriskToken,
                        Value = e
                    })
                })
                .Or(Separated(SqlParser.Comma, functionParameter));
            var functionExpression = identifier
                .And(Between(SqlParser.OpenParen, functionParameters, SqlParser.CloseParen)
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
                    }))
                .Then(e =>
                {
                    e.Item1.ChildNodes.Add(new SyntaxNode(new SyntaxToken
                    {
                        Kind = SyntaxKind.OpenParenthesisToken,
                        Value = '('
                    }));

                    foreach (var node in e.Item2)
                    {
                        e.Item1.ChildNodes.Add(node);
                    }

                    e.Item1.ChildNodes.Add(new SyntaxNode(new SyntaxToken
                    {
                        Kind = SyntaxKind.CloseParenthesisToken,
                        Value = ')'
                    }));

                    return e.Item1;
                });
            var column = functionExpression
                .Or(ZeroOrOne(identifier.And(SqlParser.Dot))
                .And(identifier)
                    .Then(e =>
                    {
                        var columnNode = new SyntaxNode(new SyntaxToken
                        {
                            Kind = SyntaxKind.IdentifierToken,
                            Value = e.Item3.Token.Value
                        });
                        var prevColumnNode = columnNode;
                        if (e.Item1 != null)
                        {
                            columnNode = new SyntaxNode(new SyntaxToken
                            {
                                Kind = SyntaxKind.DotToken,
                                Value = e.Item2
                            });

                            columnNode.ChildNodes.Add(e.Item1);
                            columnNode.ChildNodes.Add(prevColumnNode);
                        }

                        return columnNode;
                    }))
                .And(ZeroOrOne(As.And(alias))
                    .Then(e =>
                    {
                        if (e.Item2 == null)
                        {
                            return null;
                        }
                        
                        var aliasNode = new SyntaxNode(new SyntaxToken
                        {
                            Kind = SyntaxKind.AsKeyword,
                            Value = e.Item1
                        });
                        aliasNode.ChildNodes.Add(e.Item2);

                        return aliasNode;
                    }))
                .Then(e =>
                {
                    if (e.Item2 == null)
                    {
                        return e.Item1;
                    }
                    else
                    {
                        e.Item2.ChildNodes.Insert(0, e.Item1);

                        return e.Item2;
                    }
                });
            var columnsList = SqlParser.Asterisk
                .Then(e =>
                {
                    var columnNode = new SyntaxNode(new SyntaxToken
                    {
                        Kind = SyntaxKind.AsteriskToken,
                        Value = e
                    });

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
            var value = functionExpression.Or(expression)
                .And(ZeroOrOne(As.And(alias))).Then(e =>
                {
                    var valueNode = e.Item1;
                    if (e.Item2.Item1 != null)
                    {
                        var prevValueNode = valueNode;
                        valueNode = new SyntaxNode(new SyntaxToken
                        {
                            Kind = SyntaxKind.AsKeyword,
                            Value = e.Item2.Item1
                        });

                        valueNode.ChildNodes.Add(prevValueNode);
                        valueNode.ChildNodes.Add(e.Item2.Item2);
                    }

                    return valueNode;
                });
            var valuesList = Separated(SqlParser.Comma, value)
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
                    var tableName = e.Item1.Token.Value.ToString();
                    var tableNode = new SyntaxNode(new SyntaxToken
                    {
                        Kind = SyntaxKind.IdentifierToken,
                        Value = tableName
                    });

                    if (e.Item2.Item2 != null)
                    {
                        tableName = e.Item2.Item2.Token.Value.ToString();
                        var prevTableNode = tableNode;

                        tableNode = new SyntaxNode(new SyntaxToken
                        {
                            Kind = SyntaxKind.AsKeyword,
                            Value = e.Item2.Item1.Token.Value
                        });

                        tableNode.ChildNodes.Add(prevTableNode);
                        tableNode.ChildNodes.Add(new SyntaxNode(new SyntaxToken
                        {
                            Kind = e.Item2.Item2.Token.Kind,
                            Value = tableName
                        }));
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
            var selectAndFromClauses = Select
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
            var selectClause = Select
                .Then(e => new SyntaxNode(new SyntaxToken
                {
                    Kind = SyntaxKind.SelectKeyword,
                    Value = e
                })).And(valuesList);
            var columnOrValue = identifier.And(SqlParser.Dot).And(identifier).
                Then(e =>
                {
                    var node = new SyntaxNode(new SyntaxToken
                    {
                        Kind = SyntaxKind.DotToken,
                        Value = e.Item2
                    });

                    node.ChildNodes.Add(e.Item1);
                    node.ChildNodes.Add(e.Item3);

                    return node;
                }).Or(expression);
            var comparisonOperator = SqlParser.Equal.Or(SqlParser.NotEqual)
                .Or(SqlParser.LessThanOrEqual).Or(SqlParser.GreaterThanOrEqual)
                .Or(SqlParser.LessThan).Or(SqlParser.GreaterThan)
                .Then(e => new SyntaxNode(new SyntaxToken
                {
                    Kind = e switch
                    {
                        "=" => SyntaxKind.EqualsToken,
                        "<>" => SyntaxKind.NotEqualsToken,
                        "!=" => SyntaxKind.NotEqualsToken,
                        "<" => SyntaxKind.LessToken,
                        ">" => SyntaxKind.GreaterToken,
                        "<=" => SyntaxKind.LessOrEqualsToken,
                        ">=" => SyntaxKind.GreaterOrEqualsToken,
                        _ => SyntaxKind.None
                    },
                    Value = e
                }));
            var comparisonExpression = columnOrValue.And(comparisonOperator).And(columnOrValue)
                .Then(e =>
                {
                    var node = e.Item2;
                    node.ChildNodes.Add(e.Item1);
                    node.ChildNodes.Add(e.Item3);

                    return node;
                }).Or(columnOrValue);
            var logicalOperator = And.Or(OrderBy.SkipAnd(Or)).Or(Not)
                .Then(e => new SyntaxNode(new SyntaxToken
                {
                    Kind = e switch
                    {
                        "AND" => SyntaxKind.AndToken,
                        "OR" => SyntaxKind.OrToken,
                        _ => SyntaxKind.None
                    },
                    Value = e
                }));
            var logicalExpression = ZeroOrOne(Not
                    .Then(e => new SyntaxNode(new SyntaxToken
                    {
                        Kind = SyntaxKind.NotToken,
                        Value = e
                    }))).And(comparisonExpression
                .And(ZeroOrMany(logicalOperator.And(comparisonExpression)))
                .Then(e =>
                {
                    var node = e.Item1;
                    if (e.Item2.Count == 1)
                    {
                        var prevNode = node;
                        node = e.Item2[0].Item1;
                        node.ChildNodes.Add(prevNode);
                        node.ChildNodes.Add(e.Item2[0].Item2);
                    }
                    else
                    {
                        for (int i = 1; i < e.Item2.Count; i += 2)
                        {
                            var prevNode = node;
                            node = e.Item2[i].Item1;
                            node.ChildNodes.Add(prevNode);
                            node.ChildNodes.Add(e.Item2[i].Item2);
                        }
                    }

                    return node;
                }))
                .Then(e =>
                {
                    var node = e.Item2;
                    if (e.Item1 != null)
                    {
                        var prevNode = node;
                        node = e.Item1;
                        node.ChildNodes.Add(prevNode);
                    }

                    return node;
                });
            var bitwiseExpression = SqlParser.Tilda.And(expression)
                .Then(e =>
                {
                    var node = new SyntaxNode(new SyntaxToken
                    {
                        Kind = SyntaxKind.TildaToken,
                        Value = e.Item1
                    });
                    node.ChildNodes.Add(e.Item2);

                    return node;
                });
            var whereClause = Where
                .Then(e => new SyntaxNode(new SyntaxToken
                {
                    Kind = SyntaxKind.WhereKeyword,
                    Value = e
                })).And(functionExpression.Or(logicalExpression).Or(bitwiseExpression))
                .Then(e =>
                {
                    var clause = new SyntaxNode(new SyntaxToken { Kind = SyntaxKind.WhereClause });

                    clause.ChildNodes.Add(e.Item1);
                    clause.ChildNodes.Add(e.Item2);

                    return clause;
                });
            var orderByColumn = ZeroOrOne(identifier.And(SqlParser.Dot
                    .Then(e => new SyntaxNode(new SyntaxToken
                    { 
                        Kind = SyntaxKind.DotToken,
                        Value = e
                    }))))
                .And(identifier)
                .Then(e =>
                {
                    var columnNode = e.Item3;
                    if (e.Item1 != null)
                    {
                        var prevColumnNode = columnNode;
                        columnNode = e.Item2;

                        columnNode.ChildNodes.Add(e.Item1);
                        columnNode.ChildNodes.Add(prevColumnNode);
                    }

                    return columnNode;
                });
            var orderByColumns = Separated(SqlParser.Comma, orderByColumn)
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
            var orderByClause = OrderBy
                .Then(e => new SyntaxNode(new SyntaxToken
                { 
                    Kind = SyntaxKind.OrderByKeyword,
                    Value = e
                }))
                .And(orderByColumns)
                .And(ZeroOrOne(Ascending
                    .Then(e => new SyntaxNode(new SyntaxToken
                    {
                        Kind = SyntaxKind.AscendingKeyword,
                        Value = e
                    }))
                    .Or(Descending
                        .Then(e => new SyntaxNode(new SyntaxToken
                        {
                            Kind = SyntaxKind.DescendingKeyword,
                            Value = e
                        })))))
                .Then(e =>
                {
                    var clause = new SyntaxNode(new SyntaxToken { Kind = SyntaxKind.OrderByClause });
                    
                    clause.ChildNodes.Add(e.Item1);

                    foreach (var node in e.Item2)
                    {
                        clause.ChildNodes.Add(node);
                    }

                    if (e.Item3 != null)
                    {
                        clause.ChildNodes.Add(e.Item3);
                    }

                    return clause;
                });
            var selectStatement = selectAndFromClauses
                .Or(selectClause
                    .Then(e => (e.Item1, (SyntaxNode)null, (List<SyntaxNode>)null, e.Item2, (SyntaxNode)null, (List<SyntaxNode>)null)))
                .And(ZeroOrOne(whereClause.And(ZeroOrOne(orderByClause))
                    .Or(orderByClause.Then(e => ((SyntaxNode)null, e)))));

            Statement.Parser = selectStatement.Then<Statement>(e =>
            {
                if (e.Item6 == null)
                {
                    var values = e.Item4
                        .Where(n => n.Token.Kind != SyntaxKind.CommaToken)
                        .Select(e => e.Token.Value.ToString())
                        .ToList();
                    // Avoid select clause values to contain FROM
                    if (values.Contains("FROM"))
                    {
                        return null;
                    }

                    var valueAliases = e.Item4
                        .Where(n => n.Token.Kind != SyntaxKind.CommaToken && n.ChildNodes.Any(c => c.Kind == SyntaxKind.AsKeyword))
                        .Select(e => e.ChildNodes[e.ChildNodes.Count - 1].Token.Value.ToString())
                        .ToList();
                    var statement = new SelectStatement(string.Empty)
                    {
                        TableAliases = Enumerable.Empty<string>(),
                        TableNames = Enumerable.Empty<string>(),
                        ColumnAliases = Enumerable.Empty<string>(),
                        ColumnNames = Enumerable.Empty<string>()
                    };
                    var selectClause = new SyntaxNode(new SyntaxToken { Kind = SyntaxKind.SelectClause });
                    var orderByClause = new SyntaxNode(new SyntaxToken { Kind = SyntaxKind.OrderByClause });

                    selectClause.ChildNodes.Add(e.Item1);

                    foreach (var node in e.Item4)
                    {
                        selectClause.ChildNodes.Add(node);
                    }

                    statement.Nodes.Add(selectClause);

                    TryAddWhereAndOrderByClauses(statement, e.Item7);

                    return statement;
                }
                else
                {
                    var tableNames = e.Item6
                        .Where(n => n.Token.Kind != SyntaxKind.CommaToken)
                        .Select(e => e.Kind == SyntaxKind.AsKeyword
                            ? e.ChildNodes[0].Token.Value.ToString()
                            : e.Token.Value.ToString())
                        .ToList();
                    var tableAliases = e.Item6
                        .Where(n => n.Token.Kind == SyntaxKind.AsKeyword)
                        .Select(e => e.ChildNodes[1].Token.Value.ToString())
                        .ToList();
                    var columnNames = e.Item4
                        .Where(n => n.Token.Kind != SyntaxKind.CommaToken)
                        .Select(e =>
                        {
                            if (e.ChildNodes.Any())
                            {
                                if (e.ChildNodes[0].Kind == SyntaxKind.AsteriskToken)
                                {
                                    return e.ChildNodes[0].Token.Value.ToString();
                                }
                                else if (e.ChildNodes[0].Kind == SyntaxKind.AsKeyword)
                                {
                                    if (e.ChildNodes[0].ChildNodes[0].ChildNodes.Any())
                                    {
                                        return e.ChildNodes[0].ChildNodes[0].ChildNodes[1].Token.Value.ToString();
                                    }
                                    else
                                    {
                                        return e.ChildNodes[0].ChildNodes[0].Token.Value.ToString();
                                    }
                                }
                                else if (e.ChildNodes[0].Kind == SyntaxKind.DotToken)
                                {
                                    return e.ChildNodes[0].ChildNodes[1].Token.Value.ToString();
                                }
                                else
                                {
                                    return e.ChildNodes[1].Token.Value.ToString();
                                }
                            }
                            else
                            {
                                return e.Token.Value.ToString();
                            }
                        })
                        .ToList();
                    var columnAliases = e.Item4
                        .Where(n => n.Token.Kind != SyntaxKind.CommaToken && n.Kind == SyntaxKind.AsKeyword)
                        .Select(e => e.ChildNodes[1].Token.Value.ToString())
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

                    TryAddWhereAndOrderByClauses(statement, e.Item7);

                    return statement;
                }

                void TryAddWhereAndOrderByClauses(SelectStatement statement, (SyntaxNode, SyntaxNode) whereAndOrderByNodes)
                {
                    if (e.Item7.Item1 != null)
                    {
                        statement.Nodes.Add(e.Item7.Item1);
                    }

                    if (e.Item7.Item2 != null)
                    {
                        statement.Nodes.Add(e.Item7.Item2);
                    }
                }
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
