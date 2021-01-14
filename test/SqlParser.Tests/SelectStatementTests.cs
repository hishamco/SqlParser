using SqlParser.Statements;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SqlParser.Tests
{
    public class SelectStatementTests
    {
        [Theory]
        [InlineData("Select * From People", "People", new string[] { "*" })]
        [InlineData("Select FirstName From People", "People", new string[] { "FirstName" })]
        [InlineData("Select FirstName, LastName From People", "People", new string[] { "FirstName", "LastName" })]
        public async Task ParseSelectStatement(string text, string expectedTableName, string[] expectedColumnNames)
        {
            // Arrange
            var statement = new SelectStatement(text);

            // Act 
            await statement.TokenizeAsync();

            // Assert
            Assert.Equal(4, statement.Tokens.Count());
            Assert.Equal("SELECT", statement.Tokens.ElementAt(0).Value);
            Assert.Equal("FROM", statement.Tokens.ElementAt(2).Value);
            Assert.Equal(expectedTableName, statement.TableName);
            Assert.Equal(expectedColumnNames, statement.ColumnNames);
        }
    }
}
