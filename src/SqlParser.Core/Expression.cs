using SqlParser.Core.Values;
using System.Threading.Tasks;

namespace SqlParser.Core
{
    public abstract class Expression
    {
        public abstract ValueTask<SqlValue> EvaluateAsync();
    }
}
