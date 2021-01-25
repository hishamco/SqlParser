using Parlot.Fluent;
using SqlParser.Core.Syntax;
using System;
using System.Collections.Generic;
using static Parlot.Fluent.Parsers;

namespace SqlParser.Core.Statements
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

        static UpdateStatement()
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
            var columnsAssignment = Separated(Parser.Comma, identifier
                .And(Equal
                    .Then(e => new SyntaxNode(new SyntaxToken
                    {
                        Kind = SyntaxKind.EqualsToken,
                        Value = e
                    })))
                .And(terminal))
                .Then(e =>
                {
                    columns.Clear();
                    values.Clear();

                    var nodes = new List<SyntaxNode>();
                    for (int i = 0; i < e.Count; i++)
                    {
                        nodes.Add(e[i].Item1);
                        nodes.Add(e[i].Item2);
                        nodes.Add(e[i].Item3);

                        columns.Add(e[i].Item1.Token.Value.ToString());
                        values.Add(e[i].Item3.Token.Value);

                        if (i < e.Count - 1)
                        {
                            nodes.Add(new SyntaxNode(new SyntaxToken
                            {
                                Kind = SyntaxKind.CommaToken,
                                Value = e
                            }));
                        }
                    }

                    return nodes;
                });
            var upateStatement = Update
                .Then(e => new SyntaxNode(new SyntaxToken
                {
                    Kind = SyntaxKind.UpdateKeyword,
                    Value = e
                }))
                .And(identifier)
                .And(Set
                    .Then(e => new SyntaxNode(new SyntaxToken
                    {
                        Kind = SyntaxKind.SetKeyword,
                        Value = e
                    })))
                .And(columnsAssignment);

            Statement.Parser = upateStatement.Then<Statement>(e =>
            {
                var tableName = e.Item2.Token.Value.ToString();
                var statement = new UpdateStatement(tableName) { ColumnNames = columns, Values = values };
                var updateClause = new SyntaxNode(new SyntaxToken { Kind = SyntaxKind.UpdateClause });
                var setClause = new SyntaxNode(new SyntaxToken { Kind = SyntaxKind.SetClause });

                updateClause.ChildNodes.Add(e.Item1);
                updateClause.ChildNodes.Add(e.Item2);
                setClause.ChildNodes.Add(e.Item3);

                foreach (var node in e.Item4)
                {
                    setClause.ChildNodes.Add(node);
                }

                statement.Nodes.Add(updateClause);
                statement.Nodes.Add(setClause);

                return statement;
            });
        }

        public UpdateStatement(string tableName) : base(tableName)
        {

        }

        public IEnumerable<string> ColumnNames { get; set; }

        public IEnumerable<object> Values { get; set; }
    }
}
