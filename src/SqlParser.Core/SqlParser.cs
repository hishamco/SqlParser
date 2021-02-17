using Parlot;
using Parlot.Fluent;
using SqlParser.Core.Statements;
using SqlParser.Core.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using static Parlot.Fluent.Parsers;

namespace SqlParser.Core
{
    /*
     * statement ::= selectStatement | insertStatement | deleteStatement | updateStatement
     * 
     * expression ::= experssion + experssion | expression - experssion | experssion * experssion | experssion / experssion | experssion % experssion | (expression) | terminal
     *
     * terminal :: identifier | number | boolean | string | (expression)
     * 
     * identifier ::= (letter)(letter | digit)*
     * 
     * number ::= (sign)?(digit)+
     * 
     * boolean ::= True | False
     * 
     * string ::= "(letter | digit)*" | '(letter | digit)*'
     * 
     * letter ::= [a – z] | [A – Z]
     * 
     * digit ::= [0 - 9]
     * 
     * sign ::= (+ | -)
     */
    public class SqlParser : ISqlParser
    {
        internal protected static readonly Parser<char> Plus = Terms.Char('+');
        internal protected static readonly Parser<char> Minus = Terms.Char('-');
        internal protected static readonly Parser<char> Times = Terms.Char('*');
        internal protected static readonly Parser<char> Divided = Terms.Char('/');
        internal protected static readonly Parser<char> Modulo = Terms.Char('%');

        internal protected static readonly Parser<string> Equal = Terms.Text("=");
        internal protected static readonly Parser<string> NotEqual = Terms.Text("!=").Or(Terms.Text("<>"));
        internal protected static readonly Parser<string> LessThan = Terms.Text("<");
        internal protected static readonly Parser<string> GreaterThan = Terms.Text(">");
        internal protected static readonly Parser<string> LessThanOrEqual = Terms.Text("<=");
        internal protected static readonly Parser<string> GreaterThanOrEqual = Terms.Text(">=");

        internal protected static readonly Parser<char> OpenParen = Terms.Char('(');
        internal protected static readonly Parser<char> CloseParen = Terms.Char(')');
        internal protected static readonly Parser<char> Comma = Terms.Char(',');
        internal protected static readonly Parser<char> SemiColon = Terms.Char(';');
        internal protected static readonly Parser<char> Asterisk = Terms.Char('*');
        internal protected static readonly Parser<char> Dot = Terms.Char('.');

        internal protected static readonly Parser<char> Tilda = Terms.Char('~');

        internal protected static readonly Parser<string> True = Terms.Text("True", caseInsensitive: true);
        internal protected static readonly Parser<string> False = Terms.Text("False", caseInsensitive: true);

        internal protected static readonly Parser<decimal> Number = Terms.Decimal(NumberOptions.AllowSign);
        internal protected static readonly Parser<string> Boolean = True.Or(False);
        internal protected static readonly Parser<TextSpan> StringLiteral = Terms.String(StringLiteralQuotes.SingleOrDouble);
        internal protected static readonly Parser<TextSpan> Identifier = Terms.Identifier();

        public readonly Deferred<List<Statement>> Grammar = Deferred<List<Statement>>();
        public readonly Deferred<SyntaxNode> Expression = Deferred<SyntaxNode>();

        public SqlParser()
        {
            var number = Number
               .Then(e => new SyntaxNode(new SyntaxToken
               {
                   Kind = SyntaxKind.NumberToken,
                   Value = e
               }));
            var boolean = Boolean
               .Then(e => new SyntaxNode(new SyntaxToken
               {
                   Kind = SyntaxKind.BooleanToken,
                   Value = Convert.ToBoolean(e)
               }));
            var identifier = Identifier
                .Then(e => new SyntaxNode(new SyntaxToken
                {
                    Kind = SyntaxKind.IdentifierToken,
                    Value = e.ToString()
                }));
            var stringLiteral = StringLiteral
                .Then(e => new SyntaxNode(new SyntaxToken
                {
                    Kind = SyntaxKind.StringToken,
                    Value = e.ToString()
                }));
            var groupExpression = Between(OpenParen, Expression, CloseParen);
            var terminal = number
                .Or(boolean)
                .Or(stringLiteral)
                .Or(identifier)
                .Or(groupExpression);
            var unary = Recursive<SyntaxNode>(e => Minus.And(e)
                .Then(e => e.Item2).Or(terminal));
            var factor = unary.And(ZeroOrMany(Times.Or(Divided).Or(Modulo).And(unary)))
                .Then(e =>
                {
                    var node = e.Item1;
                    foreach (var op in e.Item2)
                    {
                        node = new SyntaxNode(new SyntaxToken
                        {
                            Kind = op.Item1 switch
                            {
                                '*' => SyntaxKind.TimesToken,
                                '/' => SyntaxKind.DivideToken,
                                '%' => SyntaxKind.ModuloToken,
                                _ => SyntaxKind.None
                            },
                            Value = op.Item1
                        });

                        node.ChildNodes.Add(e.Item1);
                        node.ChildNodes.Add(op.Item2);
                    }

                    return node;
                });
            Expression.Parser = factor.And(ZeroOrMany(Plus.Or(Minus).And(factor)))
                .Then(e =>
                {
                    var node = e.Item1;
                    foreach (var op in e.Item2)
                    {
                        var prevNode = node;

                        node = new SyntaxNode(new SyntaxToken
                        {
                            Kind = op.Item1 switch
                            {
                                '+' => SyntaxKind.PlusToken,
                                '-' => SyntaxKind.MinusToken,
                                _ => SyntaxKind.None
                            },
                            Value = op.Item1
                        }); ;

                        node.ChildNodes.Add(prevNode);
                        node.ChildNodes.Add(op.Item2);
                    }

                    return node;
                });
            var statement = DeleteStatement.Statement
                .Or(SelectStatement.Statement)
                .Or(InsertStatement.Statement)
                .Or(UpdateStatement.Statement);

            Grammar.Parser = Separated(SemiColon, statement);
        }

        public SyntaxTree Parse(string commandText)
        {
            Grammar.TryParse(commandText, out List<Statement> statements);

            var syntaxTree = new SyntaxTree();
            if (statements != null)
            {
                foreach (var statement in statements.Where(s => s != null))
                {
                    syntaxTree.Statements.Add(statement);
                }
            }

            return syntaxTree;
        }
    }
}
