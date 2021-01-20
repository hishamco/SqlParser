using Parlot;
using Parlot.Fluent;

namespace SqlParser.Core
{
    public class SqlContext : ParseContext
    {
        public SqlContext(string text) : base(new Scanner(text))
        {
        }
    }
}
