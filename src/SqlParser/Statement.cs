using System.Collections.Generic;
using System.Threading.Tasks;

namespace SqlParser
{
    public abstract class Statement
    {
        protected Statement(string commandText)
        {
            CommandText = commandText;
        }

        protected string CommandText { get; }

        public IEnumerable<Token> Tokens { get; protected set; }

        public abstract Task TokenizeAsync();
    }
}
