namespace SqlParser.Core.Syntax
{
    public struct SyntaxToken
    {
        public SyntaxKind Kind { get; set; }

        public object Value { get; set; }
    }
}
