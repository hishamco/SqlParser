using System;

namespace SqlParser.Core.Values
{
    public abstract class SqlValue : IEquatable<SqlValue>
    {
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

        public virtual bool ToBooleanValue() => false;

        public virtual decimal ToNumberValue() => 0M;

        public virtual string ToStringValue() => String.Empty;

        public abstract bool Equals(SqlValue other);
    }
}
