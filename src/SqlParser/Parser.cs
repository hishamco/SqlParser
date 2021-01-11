using Parlot;
using Parlot.Fluent;
using SqlParser.Expressions;
using System;
using static Parlot.Fluent.Parsers;

namespace SqlParser
{
    /*
     * expression :: identifier | number | boolean | string | (expression)
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
        protected static readonly Parser<string> True = Terms.Text("True", caseInsensitive: true);
        protected static readonly Parser<string> False = Terms.Text("False", caseInsensitive: true);

        public static readonly Deferred<Expression> Expression = Deferred<Expression>();

        static Parser()
        {
            var number = Terms.Decimal(NumberOptions.AllowSign)
                .Then<Expression>(e => new NumericExpression(e));
            var boolean = True.Or(False)
                .Then<Expression>(e => new BooleanExpression(Convert.ToBoolean(e)));
            var stringLiteral = Terms.String(StringLiteralQuotes.SingleOrDouble)
                .Then<Expression>(e => new LiteralExpression(e.Buffer.Substring(1, e.Buffer.Length - 2)));
            var identifier = Terms.Identifier()
                .Then<Expression>(e => new IdentifierExpression(e.Buffer));

            Expression.Parser = number.Or(boolean).Or(stringLiteral).Or(identifier);
        }
    }
}
