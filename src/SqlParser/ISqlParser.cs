using System.Collections.Generic;

namespace SqlParser
{
    public interface ISqlParser
    {
        IEnumerable<Statement> Parse(string commandText);
    }
}
