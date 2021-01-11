using System;

namespace SqlParser.Values
{
    public sealed class NumberValue : SqlValue
    {
        private readonly decimal _value;

        public static readonly NumberValue Zero = new NumberValue(0M);

        public NumberValue(decimal value) : base(value)
        {
            _value = value;
        }

        public override bool Equals(SqlValue other)
            => other is NumberValue
                ? _value == Convert.ToDecimal(other.Value)
                : false;
    }
}
