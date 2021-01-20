namespace SqlParser.Core.Expressions
{
    public abstract class UnaryExpression : Expression
    {
        public UnaryExpression(Expression innerExpression)
        {
            InnerExpression = innerExpression;
        }

        public Expression InnerExpression { get; set; }
    }
}
