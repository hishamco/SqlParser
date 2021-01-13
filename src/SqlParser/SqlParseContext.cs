using Parlot;
using Parlot.Fluent;

namespace SqlParser
{
    public class SqlParseContext : ParseContext
    {
        public SqlParseContext(string text) : base(new Scanner(text))
        {
        }
    }
}
