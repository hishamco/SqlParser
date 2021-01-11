using SqlParser.Values;
using System.Threading.Tasks;

namespace SqlParser.Expressions
{
    public class ModulusExpression : BinaryExpression
    {
        public ModulusExpression(Expression left, Expression right) : base(left, right)
        {
        }

        public async override ValueTask<SqlValue> EvaluateAsync()
        {
            var left = await Left.EvaluateAsync();
            var right = await Right.EvaluateAsync();
            var modulus = left.ToNumberValue() % right.ToNumberValue();

            return new NumberValue(modulus);
        }
    }
}
