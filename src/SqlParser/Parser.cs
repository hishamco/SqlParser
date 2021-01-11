using Parlot;
using Parlot.Fluent;
using SqlParser.Expressions;
using System;
using static Parlot.Fluent.Parsers;

namespace SqlParser
{
    /*
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
    public class Parser
    {
        protected static readonly Parser<char> Plus = Terms.Char('+');
        protected static readonly Parser<char> Minus = Terms.Char('-');
        protected static readonly Parser<char> Times = Terms.Char('*');
        protected static readonly Parser<char> Divided = Terms.Char('/');
        protected static readonly Parser<char> Modulo = Terms.Char('%');

        protected static readonly Parser<char> OpenParen = Terms.Char('(');
        protected static readonly Parser<char> CloseParen = Terms.Char(')');

        protected static readonly Parser<string> True = Terms.Text("True", caseInsensitive: true);
        protected static readonly Parser<string> False = Terms.Text("False", caseInsensitive: true);

        public static readonly Deferred<Expression> Expression;

        static Parser()
        {
            var expression = Deferred<Expression>();
            var number = Terms.Decimal(NumberOptions.AllowSign)
                .Then<Expression>(e => new NumericExpression(e));
            var boolean = True.Or(False)
                .Then<Expression>(e => new BooleanExpression(Convert.ToBoolean(e)));
            var stringLiteral = Terms.String(StringLiteralQuotes.SingleOrDouble)
                .Then<Expression>(e => new LiteralExpression(e.Buffer.Substring(1, e.Buffer.Length - 2)));
            var identifier = Terms.Identifier()
                .Then<Expression>(e => new IdentifierExpression(e.Buffer));
            var groupExpression = Between(OpenParen, expression, CloseParen);
            var terminal = number.Or(boolean).Or(stringLiteral).Or(identifier).Or(groupExpression);
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

            expression.Parser = factor.And(ZeroOrMany(Plus.Or(Minus).And(factor)))
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

            Expression = expression;
        }
    }
}
