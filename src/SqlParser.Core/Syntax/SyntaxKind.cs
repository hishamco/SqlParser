namespace SqlParser.Core.Syntax
{
    public enum SyntaxKind
    {
        None,

        // Tokens
        BooleanToken,
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
        OrderByKeyword,
        AscendingKeyword,
        DescendingKeyword,
        UpdateKeyword,
        SetKeyword,
        DistinctKeyword,
        TopKeyword,

        // Clauses
        DeleteClause,
        FromClause,
        InsertIntoClause,
        ValuesClause,
        SelectClause,
        OrderByClause,
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