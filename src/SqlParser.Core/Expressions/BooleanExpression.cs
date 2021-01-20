using SqlParser.Core.Values;
using System.Threading.Tasks;

namespace SqlParser.Core.Expressions
{
    public class BooleanExpression : Expression
    {
        private readonly BooleanValue _value;

        public BooleanExpression(bool value) : this(new BooleanValue(value))
        {

        }

        public BooleanExpression(BooleanValue value)
        {
            _value = value;
        }

        public override ValueTask<SqlValue> EvaluateAsync() => new ValueTask<SqlValue>(_value);
    }
}
