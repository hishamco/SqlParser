using SqlParser.Values;
using System.Threading.Tasks;

namespace SqlParser
{
    public abstract class Expression
    {
        public abstract ValueTask<SqlValue> EvaluateAsync();
    }
}
