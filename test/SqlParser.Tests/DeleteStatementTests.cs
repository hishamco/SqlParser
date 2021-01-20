using Parlot;
using SqlParser.Statements;
using System.Linq;
using Xunit;

namespace SqlParser.Tests
{
    public class DeleteStatementTests
    {
        [Theory]
        [InlineData("DELETE FROM Customers", "Customers")]
        [InlineData("delete from Customers", "Customers")]
        public void ParseDeleteStatement(string text, string expected)
        {
            // Arrange
            var context = new SqlContext(text);
            var result = new ParseResult<Statement>();

            // Act
            DeleteStatement.Statement.Parse(context, ref result);

            // Assert
            var statement = result.Value as DeleteStatement;
            Assert.Equal(3, statement.Tokens.Count());
            Assert.Equal("DELETE", statement.Tokens.ElementAt(0).Value);
            Assert.Equal("FROM", statement.Tokens.ElementAt(1).Value);
            Assert.Equal(expected, statement.TableName);
        }
    }
}
