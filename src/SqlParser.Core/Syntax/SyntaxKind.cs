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
        EqualsToken,
        NotEqualsToken,
        LessToken,
        LessOrEqualsToken,
        GreaterToken,
        GreaterOrEqualsToken,
        AndToken,
        OrToken,
        NotToken,
        OpenParenthesisToken,
        CloseParenthesisToken,
        ColonToken,
        CommaToken,
        DotToken,
        AsteriskToken,
        IdentifierToken,
        TildaToken,

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
        WhereKeyword,
        LikeKeyword,
        BetweenKeyword,
        InKeyword,

        // Clauses
        DeleteClause,
        FromClause,
        InsertIntoClause,
        ValuesClause,
        SelectClause,
        OrderByClause,
        UpdateClause,
        SetClause,
        WhereClause,

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