using Parlot;
using Parlot.Fluent;
using SqlParser.Expressions;
using SqlParser.Statements;
using System;
using System.Collections.Generic;
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

        internal protected static readonly Parser<string> True = Terms.Text("True", caseInsensitive: true);
        internal protected static readonly Parser<string> False = Terms.Text("False", caseInsensitive: true);

        internal static readonly Parser<decimal> Number = Terms.Decimal(NumberOptions.AllowSign);
        internal static readonly Parser<string> Boolean = True.Or(False);
        internal static readonly Parser<TextSpan> StringLiteral = Terms.String(StringLiteralQuotes.SingleOrDouble);
        internal static readonly Parser<TextSpan> Identifier = Terms.Identifier();
        internal static readonly Deferred<Expression> Terminal = Deferred<Expression>();

        public readonly Deferred<Expression> Expression = Deferred<Expression>();

        public Parser()
        {
            var groupExpression = Between(OpenParen, Expression, CloseParen);

            Terminal.Parser = Number.Then<Expression>(e => new NumericExpression(e))
                .Or(Boolean.Then<Expression>(e => new BooleanExpression(Convert.ToBoolean(e))))
                .Or(StringLiteral.Then<Expression>(e => new LiteralExpression(e.ToString())))
                .Or(Identifier.Then<Expression>(e => new IdentifierExpression(e.ToString())))
                .Or(groupExpression);

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

        public IEnumerable<Statement> Parse(string commandText)
        {
            var statements = new List<Statement>();
            Statement statement = null;
            if (commandText.StartsWith("SELECT"))
            {
                statement = new SelectStatement(commandText);
            }
            else if (commandText.StartsWith("INSERT"))
            {
                statement = new InsertStatement(commandText);
            }
            else if (commandText.StartsWith("DELETE"))
            {
                statement = new DeleteStatement(commandText);
            }
            else if (commandText.StartsWith("UPDATE"))
            {
                statement = new UpdateStatement(commandText);
            }

            try
            {
                statement.TokenizeAsync();

                if (statement.Tokens != null)
                {
                    statements.Add(statement);
                }
            }
            catch
            {

            }

            return statements;
        }
    }
}
