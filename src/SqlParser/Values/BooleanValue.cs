namespace SqlParser.Values
{
    public sealed class BooleanValue : SqlValue
    {
        private readonly bool _value;

        public static readonly BooleanValue False = new BooleanValue(false);
        public static readonly BooleanValue True = new BooleanValue(true);

        public BooleanValue(bool value)
        {
            _value = value;
        }

        public override bool ToBooleanValue() => _value;

        public override bool Equals(SqlValue other)
            => other is BooleanValue
                ? _value == other.ToBooleanValue()
                : false;
    }
}
