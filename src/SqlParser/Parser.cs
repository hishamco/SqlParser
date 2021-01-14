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
        internal protected static readonly Parser<char> Plus = Terms.Char('+');
        internal protected static readonly Parser<char> Minus = Terms.Char('-');
        internal protected static readonly Parser<char> Times = Terms.Char('*');
        internal protected static readonly Parser<char> Divided = Terms.Char('/');
        internal protected static readonly Parser<char> Modulo = Terms.Char('%');

        internal protected static readonly Parser<char> OpenParen = Terms.Char('(');
        internal protected static readonly Parser<char> CloseParen = Terms.Char(')');
        internal protected static readonly Parser<char> Comma = Terms.Char(',');

        internal protected static readonly Parser<string> True = Terms.Text("True", caseInsensitive: true);
        internal protected static readonly Parser<string> False = Terms.Text("False", caseInsensitive: true);

        internal static readonly Parser<Expression> Number;
        internal static readonly Parser<Expression> Boolean;
        internal static readonly Parser<Expression> StringLiteral;
        internal static readonly Parser<Expression> Identifier;
        internal static readonly Parser<Expression> Terminal;

        public static readonly Deferred<Expression> Expression = Deferred<Expression>();

        static Parser()
        {
            Number = Terms.Decimal(NumberOptions.AllowSign).Then<Expression>(e => new NumericExpression(e));
            Boolean = True.Or(False).Then<Expression>(e => new BooleanExpression(Convert.ToBoolean(e)));
            StringLiteral = Terms.String(StringLiteralQuotes.SingleOrDouble).Then<Expression>(e => new LiteralExpression(e.ToString()));
            Identifier = Terms.Identifier().Then<Expression>(e => new IdentifierExpression(e.ToString()));
            
            var groupExpression = Between(OpenParen, Expression, CloseParen);
            Terminal = Number.Or(Boolean).Or(StringLiteral).Or(Identifier).Or(groupExpression);

            var unary = Recursive<Expression>(e => Minus.And(e)
                .Then<Expression>(e => new NegateExpression(e.Item2)).Or(Terminal));
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
        }
    }
}
