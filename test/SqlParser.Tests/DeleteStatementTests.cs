using SqlParser.Statements;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SqlParser.Tests
{
    public class DeleteStatementTests
    {
        [Theory]
        [InlineData("DELETE FROM Customers", "Customers")]
        [InlineData("delete from Customers", "Customers")]
        public async Task ParseStringExpression(string text, string expected)
        {
            // Arrange
            var statement = new DeleteStatement(text);

            // Act 
            await statement.TokenizeAsync();

            // Assert
            Assert.Equal(3, statement.Tokens.Count());
            Assert.Equal("DELETE", statement.Tokens.ElementAt(0));
            Assert.Equal("FROM", statement.Tokens.ElementAt(1));
            Assert.Equal(expected, statement.TableName);
        }
    }
}
