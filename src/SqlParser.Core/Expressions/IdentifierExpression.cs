using System;

namespace SqlParser.Core.Expressions
{
    public class IdentifierExpression : LiteralExpression
    {
        public IdentifierExpression(string name) : base(name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public string Name { get; }
    }
}
