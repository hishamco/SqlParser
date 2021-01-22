using Parlot;
using SqlParser.Core;
using SqlParser.Core.Statements;
using System.Linq;
using Xunit;

namespace SqlParser.Tests
{
    public class SelectStatementTests
    {
        [Theory]
        [InlineData("Select * From People", "People", new string[] { "*" })]
        [InlineData("Select FirstName From People", "People", new string[] { "FirstName" })]
        [InlineData("Select FirstName, LastName From People", "People", new string[] { "FirstName", "LastName" })]
        public void ParseSelectStatement(string text, string expectedTableName, string[] expectedColumnNames)
        {
            // Arrange
            var context = new SqlContext(text);
            var result = new ParseResult<Statement>();

            // Act
            SelectStatement.Statement.Parse(context, ref result);

            // Assert
            var statement = result.Value as SelectStatement;
            Assert.Equal(4, statement.Tokens.Count());
            Assert.Equal("SELECT", statement.Tokens.ElementAt(0).Value);
            Assert.Equal("FROM", statement.Tokens.ElementAt(2).Value);
            Assert.Equal(expectedTableName, statement.TableName);
            Assert.Equal(expectedColumnNames, statement.ColumnNames);
        }

        [Theory]
        [InlineData("Select People.FirstName From People", new string[] { "People.FirstName" })]
        [InlineData("Select People.FirstName, People.LastName From People", new string[] { "People.FirstName", "People.LastName" })]
        public void ParseFullQualifiedColumnNames(string text, string[] expectedColumnNames)
        {
            // Arrange
            var context = new SqlContext(text);
            var result = new ParseResult<Statement>();

            // Act
            SelectStatement.Statement.Parse(context, ref result);

            // Assert
            var statement = result.Value as SelectStatement;
            Assert.Equal(expectedColumnNames, statement.ColumnNames.ToArray());
        }
    }
}
