using SqlParser.Core.Values;
using System.Threading.Tasks;

namespace SqlParser.Core.Expressions
{
    public class NegateExpression : UnaryExpression
    {
        public NegateExpression(Expression innerExpression) : base(innerExpression)
        {
        }

        public async override ValueTask<SqlValue> EvaluateAsync()
        {
            var innerExpressionValue = await InnerExpression.EvaluateAsync();

            return new NumberValue(-1 * innerExpressionValue.ToNumberValue());
        }
    }
}
