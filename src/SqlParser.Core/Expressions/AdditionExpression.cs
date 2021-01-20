using SqlParser.Core.Values;
using System;
using System.Threading.Tasks;

namespace SqlParser.Core.Expressions
{
    public class AdditionExpression : BinaryExpression
    {
        public AdditionExpression(Expression left, Expression right) : base(left, right)
        {
        }

        public async override ValueTask<SqlValue> EvaluateAsync()
        {
            var left = await Left.EvaluateAsync();
            var right = await Right.EvaluateAsync();
            var addition = left.ToNumberValue() + right.ToNumberValue();

            return new NumberValue(addition);
        }
    }
}
