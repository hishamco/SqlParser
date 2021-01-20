using System.Collections.Generic;

namespace SqlParser.Core
{
    public interface ISqlParser
    {
        IEnumerable<Statement> Parse(string commandText);
    }
}
