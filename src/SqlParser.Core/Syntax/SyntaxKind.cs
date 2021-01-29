namespace SqlParser.Core.Syntax
{
    public enum SyntaxKind
    {
        // Tokens
        NumberToken,
        StringToken,
        PlusToken,
        MinusToken,
        TimesToken,
        DivideToken,
        ModuloToken,
        StarToken,
        EqualsToken,
        OpenParenthesisToken,
        CloseParenthesisToken,
        ColonToken,
        CommaToken,
        DotToken,
        AsteriskToken,
        IdentifierToken,

        // Keywords
        DeleteKeyword,
        FromKeyword,
        InsertKeyword,
        IntoKeyword,
        ValuesKeyword,
        SelectKeyword,
        AsKeyword,
        UpdateKeyword,
        SetKeyword,
        DistinctKeyword,

        // Clauses
        DeleteClause,
        FromClause,
        InsertIntoClause,
        ValuesClause,
        SelectClause,
        UpdateClause,
        SetClause,

        // Statements
        DeleteStatement,
        InsertStatement,
        SelectStatement,
        UpdateStatement,

        // Expressions
        LiteralExpression,
        NameExpression
    }
}