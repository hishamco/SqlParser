using SqlParser.Core.Values;
using System.Threading.Tasks;

namespace SqlParser.Core.Expressions
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
            var subtraction = left.ToNumberValue() - right.ToNumberValue();

            return new NumberValue(subtraction);
        }
    }
}
