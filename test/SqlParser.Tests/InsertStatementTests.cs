using Parlot;
using SqlParser.Statements;
using System;
using System.Linq;
using Xunit;

namespace SqlParser.Tests
{
    public class InsertStatementTests
    {
        [Theory]
        [InlineData("Insert Into People (FirstName) VALUES ('Jon')", "People", new[] { "FirstName"}, new[] { "Jon" })]
        [InlineData("Insert Into People (FirstName, LastName, Age) VALUES ('Jon', 'Doe', 32)", "People", new[] { "FirstName", "LastName", "Age" }, new object[] { "Jon", "Doe", 32 })]
        [InlineData("Insert Into People VALUES ('Jon')", "People", new string[] { }, new[] { "Jon" })]
        public void ParseInsertStatement(string text, string expectedTableName, string[] expectedColumnNames, object[] expectedValues)
        {
            // Arrange
            var context = new SqlContext(text);
            var result = new ParseResult<Statement>();

            // Act
            InsertStatement.Statement.Parse(context, ref result);

            // Assert
            var statement = result.Value as InsertStatement;
            Assert.Equal(6, statement.Tokens.Count());
            Assert.Equal("INSERT", statement.Tokens.ElementAt(0).Value);
            Assert.Equal("INTO", statement.Tokens.ElementAt(1).Value);
            Assert.Equal(expectedTableName, statement.TableName);
            Assert.Equal(expectedColumnNames, statement.ColumnNames);
            Assert.True(expectedValues.SequenceEqual(statement.Tokens.Last().Value as object[], new SequenceComparer()));
        }
    }
}
