using System;

namespace SqlParser.Values
{
    public class StringValue : SqlValue
    {
        private readonly string _value;

        public static StringValue Empty = new StringValue(String.Empty);

        public StringValue(string value) : base(value)
        {
            _value = value;
        }

        public override bool Equals(SqlValue other)
            => other is StringValue
                ? _value == Convert.ToString(other.Value)
                : false;
    }
}
