using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace SqlParser.Tests
{
    public class SequenceComparer : IEqualityComparer<object>
    {
        public new bool Equals([AllowNull] object x, [AllowNull] object y)
        {
            // HACK: Ensure that integer treated as decimal
            if (x is int && y is decimal)
            {
                return true;
            }

            return x.Equals(y);
        }

        public int GetHashCode([DisallowNull] object obj) => obj.GetHashCode();
    }
}
