using SqlParser.Core.Values;
using System.Threading.Tasks;

namespace SqlParser.Core.Expressions
{
    public class LiteralExpression : Expression
    {
        private readonly StringValue _value;

        public LiteralExpression(string value) : this(new StringValue(value))
        {

        }

        public LiteralExpression(StringValue value)
        {
            _value = value;
        }

        public override ValueTask<SqlValue> EvaluateAsync() => new ValueTask<SqlValue>(_value);
    }
}
