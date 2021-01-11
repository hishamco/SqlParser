using SqlParser.Values;
using System;
using System.Threading.Tasks;

namespace SqlParser.Expressions
{
    public class SubtractionExpression : BinaryExpression
    {
        public SubtractionExpression(Expression left, Expression right) : base(left, right)
        {
        }

        public async override ValueTask<SqlValue> EvaluateAsync()
        {
            var left = await Left.EvaluateAsync();
            var right = await Right.EvaluateAsync();
            var subtraction = Convert.ToDecimal(left.Value) - Convert.ToDecimal(right.Value);

            return new NumberValue(subtraction);
        }
    }
}
