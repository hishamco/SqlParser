using System;

namespace SqlParser.Values
{
    public abstract class SqlValue : IEquatable<SqlValue>
    {
        public SqlValue(object value)
        {
            Value = value;
        }

        public object Value { get; }

        public static SqlValue Create(object value)
        {
            if (value is SqlValue sqlValue)
            {
                return sqlValue;
            }

            var valueType = value.GetType();
            switch (Type.GetTypeCode(valueType))
            {
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return new NumberValue(Convert.ToDecimal(value));
                case TypeCode.Char:
                case TypeCode.String:
                    return new StringValue(Convert.ToString(value));
                default:
                    throw new InvalidOperationException();
            }
        }

        public abstract bool Equals(SqlValue other);
    }
}
