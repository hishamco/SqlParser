using Parlot;
using SqlParser.Expressions;
using SqlParser.Values;
using System.Threading.Tasks;
using Xunit;

namespace SqlParser.Tests
{
    public class ParserTests
    {
        [Theory]
        [InlineData("12", 12)]
        [InlineData("12.5", 12.5)]
        public async Task ParseNumberExpression(string text, decimal expected)
        {
            // Arrange
            var context = new SqlParseContext(text);
            var result = new ParseResult<Expression>();

            // Arrange
            Parser.Expression.Parse(context, ref result);

            var value = await (result.Value as NumericExpression).EvaluateAsync();

            // Assert
            Assert.Equal(new NumberValue(expected), value);
        }

        [Theory]
        [InlineData("TRUE", true)]
        [InlineData("trUE", true)]
        [InlineData("false", false)]
        [InlineData("False", false)]
        public async Task ParseBooleanExpression(string text, bool expected)
        {
            // Arrange
            var context = new SqlParseContext(text);
            var result = new ParseResult<Expression>();

            // Arrange
            Parser.Expression.Parse(context, ref result);

            var value = await (result.Value as BooleanExpression).EvaluateAsync();

            // Assert
            Assert.Equal(new BooleanValue(expected), value);
        }

        [Theory]
        [InlineData("\"Hisham\"", "Hisham")]
        [InlineData("\'Hisham\'", "Hisham")]
        public async Task ParseStringExpression(string text, string expected)
        {
            // Arrange
            var context = new SqlParseContext(text);
            var result = new ParseResult<Expression>();

            // Arrange
            Parser.Expression.Parse(context, ref result);

            var value = await (result.Value as LiteralExpression).EvaluateAsync();

            // Assert
            Assert.Equal(new StringValue(expected), value);
        }

        [Theory]
        [InlineData("Name", "Name")]
        [InlineData("Name123", "Name123")]
        [InlineData("_Name", "_Name")]
        [InlineData("First_Name", "First_Name")]
        public async Task ParseIdentifierExpression(string text, string expected)
        {
            // Arrange
            var context = new SqlParseContext(text);
            var result = new ParseResult<Expression>();

            // Arrange
            Parser.Expression.Parse(context, ref result);

            var value = await(result.Value as LiteralExpression).EvaluateAsync();

            // Assert
            Assert.Equal(new StringValue(expected), value);
        }

        [Theory]
        [InlineData("5+2", 7)]
        [InlineData("5-2", 3)]
        [InlineData("5*2", 10)]
        [InlineData("-5*2", -10)]
        [InlineData("5/2", 2.5)]
        [InlineData("5%2", 1)]
        [InlineData("12*3-6+70", 100)]
        [InlineData("150+50-5*5", 175)]
        public async Task EvaluateArithmaticExpression(string text, decimal expected)
        {
            // Arrange
            var context = new SqlParseContext(text);
            var result = new ParseResult<Expression>();

            // Arrange
            Parser.Expression.Parse(context, ref result);

            var value = await(result.Value as BinaryExpression).EvaluateAsync();

            // Assert
            Assert.Equal(new NumberValue(expected), value);
        }
    }
}
