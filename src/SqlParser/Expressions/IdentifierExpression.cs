using System;

namespace SqlParser.Expressions
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
