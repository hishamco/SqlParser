using SqlParser.Values;
using System.Threading.Tasks;

namespace SqlParser.Expressions
{
    public class DivisionExpression : BinaryExpression
    {
        public DivisionExpression(Expression left, Expression right) : base(left, right)
        {
        }

        public async override ValueTask<SqlValue> EvaluateAsync()
        {
            var left = await Left.EvaluateAsync();
            var right = await Right.EvaluateAsync();
            var division = left.ToNumberValue() / right.ToNumberValue();

            return new NumberValue(division);
        }
    }
}
