using SqlParser.Statements;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SqlParser.Tests
{
    public class UpdateStatementTests
    {
        [Theory]
        [InlineData("Update People Set Age = 32", "People", new[] { "Age" }, new object[] { 32 })]
        [InlineData("Update People Set FirstName = 'Jon', LastName = 'Doe'", "People", new [] { "FirstName", "LastName" }, new object[] { "Jon", "Doe" })]
        public async Task ParseUpdateStatement(string text, string expectedTableName, string[] expectedColumnNames, object[] expectedValues)
        {
            // Arrange
            var statement = new UpdateStatement(text);

            // Act 
            await statement.TokenizeAsync();

            // Assert
            Assert.Equal(5, statement.Tokens.Count());
            Assert.Equal("UPDATE", statement.Tokens.ElementAt(0).Value);
            Assert.Equal("SET", statement.Tokens.ElementAt(2).Value);
            Assert.Equal(expectedTableName, statement.TableName);
            Assert.Equal(expectedColumnNames, statement.ColumnNames);
            Assert.True(expectedValues.SequenceEqual(statement.Values, new SequenceComparer()));
        }
    }
}
