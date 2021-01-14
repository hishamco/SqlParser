using SqlParser.Statements;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SqlParser.Tests
{
    public class InsertStatementTests
    {
        [Theory]
        [InlineData("Insert Into People (FirstName) VALUES ('Jon')", "People", new[] { "FirstName"}, new[] { "Jon" })]
        [InlineData("Insert Into People (FirstName, LastName, Age) VALUES ('Jon', 'Doe', 32)", "People", new[] { "FirstName", "LastName", "Age" }, new object[] { "Jon", "Doe", 32 })]
        [InlineData("Insert Into People VALUES ('Jon')", "People", new string[] { }, new[] { "Jon" })]
        public async Task ParseInsertStatement(string text, string expectedTableName, string[] expectedColumnNames, object[] expectedValues)
        {
            // Arrange
            var statement = new InsertStatement(text);

            // Act 
            await statement.TokenizeAsync();

            // Assert
            Assert.Equal(6, statement.Tokens.Count());
            Assert.Equal("INSERT", statement.Tokens.ElementAt(0).Value);
            Assert.Equal("INTO", statement.Tokens.ElementAt(1).Value);
            Assert.Equal(expectedTableName, statement.TableName);
            Assert.Equal(expectedColumnNames, statement.ColumnNames);
            Assert.True(expectedValues.SequenceEqual(statement.Tokens.Last().Value as object[], new SequenceComparer()));
        }
    }
}
