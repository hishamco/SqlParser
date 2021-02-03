using SqlParser.Core.Syntax;

namespace SqlParser.Core
{
    public interface ISqlParser
    {
        SyntaxTree Parse(string commandText); 
    }
}
