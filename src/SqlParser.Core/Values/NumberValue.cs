namespace SqlParser.Core.Values
{
    public sealed class NumberValue : SqlValue
    {
        private readonly decimal _value;

        public static readonly NumberValue Zero = new NumberValue(0M);

        public NumberValue(decimal value)
        {
            _value = value;
        }

        public override decimal ToNumberValue() => _value;

        public override bool Equals(SqlValue other)
            => other is NumberValue
                ? _value == other.ToNumberValue()
                : false;
    }
}
