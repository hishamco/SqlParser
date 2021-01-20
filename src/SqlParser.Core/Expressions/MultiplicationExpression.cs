using SqlParser.Core.Values;
using System.Threading.Tasks;

namespace SqlParser.Core.Expressions
{
    public class MultiplicationExpression : BinaryExpression
    {
        public MultiplicationExpression(Expression left, Expression right) : base(left, right)
        {
        }

        public async override ValueTask<SqlValue> EvaluateAsync()
        {
            var left = await Left.EvaluateAsync();
            var right = await Right.EvaluateAsync();
            var multiplication = left.ToNumberValue() * right.ToNumberValue();

            return new NumberValue(multiplication);
        }
    }
}
