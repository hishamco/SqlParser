using Parlot;
using Parlot.Fluent;
using SqlParser.Expressions;
using SqlParser.Statements;
using System;
using System.Collections.Generic;
using System.Linq;
using static Parlot.Fluent.Parsers;

namespace SqlParser
{
    /*
     * statement ::= selectStatement | insertStatement | deleteStatement | updateStatement
     * 
     * expression ::= experssion + factor | expression - factor | factor
     * 
     * factor ::= factor * terminal | factor / terminal | factor % terminal | terminal
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
    public class Parser : ISqlParser
    {
        internal protected static readonly Parser<char> Plus = Terms.Char('+');
        internal protected static readonly Parser<char> Minus = Terms.Char('-');
        internal protected static readonly Parser<char> Times = Terms.Char('*');
        internal protected static readonly Parser<char> Divided = Terms.Char('/');
        internal protected static readonly Parser<char> Modulo = Terms.Char('%');

        internal protected static readonly Parser<char> OpenParen = Terms.Char('(');
        internal protected static readonly Parser<char> CloseParen = Terms.Char(')');
        internal protected static readonly Parser<char> Comma = Terms.Char(',');
        internal protected static readonly Parser<char> SemiColon = Terms.Char(';');
        internal protected static readonly Parser<char> Asterisk = Terms.Char('*');

        internal protected static readonly Parser<string> True = Terms.Text("True", caseInsensitive: true);
        internal protected static readonly Parser<string> False = Terms.Text("False", caseInsensitive: true);

        internal protected static readonly Parser<decimal> Number = Terms.Decimal(NumberOptions.AllowSign);
        internal protected static readonly Parser<string> Boolean = True.Or(False);
        internal protected static readonly Parser<TextSpan> StringLiteral = Terms.String(StringLiteralQuotes.SingleOrDouble);
        internal protected static readonly Parser<TextSpan> Identifier = Terms.Identifier();

        public readonly Deferred<Expression> Expression = Deferred<Expression>();
        public readonly Deferred<List<Statement>> Grammar = Deferred<List<Statement>>();

        public Parser()
        {
            var number = Number
                .Then<Expression>(e => new NumericExpression(e));
            var boolean = Boolean.Then<Expression>(e => new BooleanExpression(Convert.ToBoolean(e)));
            var stringLiteral = StringLiteral.Then<Expression>(e => new LiteralExpression(e.ToString()));
            var identifier = Identifier.Then<Expression>(e => new IdentifierExpression(e.ToString()));
            var groupExpression = Between(OpenParen, Expression, CloseParen);
            var terminal = number
                .Or(boolean)
                .Or(stringLiteral)
                .Or(identifier)
                .Or(groupExpression);

            var unary = Recursive<Expression>(e => Minus.And(e)
                .Then<Expression>(e => new NegateExpression(e.Item2)).Or(terminal));
            var factor = unary.And(ZeroOrMany(Times.Or(Divided).Or(Modulo).And(unary)))
                .Then(e =>
                {
                    var result = e.Item1;
                    foreach (var op in e.Item2)
                    {
                        result = op.Item1 switch
                        {
                            '*' => new MultiplicationExpression(result, op.Item2),
                            '/' => new DivisionExpression(result, op.Item2),
                            '%' => new ModulusExpression(result, op.Item2),
                            _ => null
                        };
                    }

                    return result;
                });

            Expression.Parser = factor.And(ZeroOrMany(Plus.Or(Minus).And(factor)))
                .Then(e =>
                {
                    var result = e.Item1;
                    foreach (var op in e.Item2)
                    {
                        result = op.Item1 switch
                        {
                            '+' => new AdditionExpression(result, op.Item2),
                            '-' => new SubtractionExpression(result, op.Item2),
                            _ => null
                        };
                    }

                    return result;
                });

            var statement = DeleteStatement.Statement
                .Or(SelectStatement.Statement)
                .Or(InsertStatement.Statement)
                .Or(UpdateStatement.Statement);

            Grammar.Parser = Separated(SemiColon, statement);
        }

        public IEnumerable<Statement> Parse(string commandText)
        {
            Grammar.TryParse(commandText, out List<Statement> statements);

            return statements ?? Enumerable.Empty<Statement>();
        }
    }
}
