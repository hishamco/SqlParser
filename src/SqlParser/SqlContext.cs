using Parlot;
using Parlot.Fluent;

namespace SqlParser
{
    public class SqlContext : ParseContext
    {
        public SqlContext(string text) : base(new Scanner(text))
        {
        }
    }
}
