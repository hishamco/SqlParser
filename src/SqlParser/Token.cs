namespace SqlParser
{
    public struct Token
    {
        public TokenType Type { get; set; }

        public object Value { get; set; }
    }
}
