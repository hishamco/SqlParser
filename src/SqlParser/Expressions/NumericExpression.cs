using SqlParser.Values;
using System.Threading.Tasks;

namespace SqlParser.Expressions
{
    public class NumericExpression : Expression
    {
        private readonly NumberValue _value;

        public NumericExpression(decimal value) : this(new NumberValue(value))
        {

        }

        public NumericExpression(NumberValue value)
        {
            _value = value;
        }

        public override ValueTask<SqlValue> EvaluateAsync() => new ValueTask<SqlValue>(_value);
    }
}
