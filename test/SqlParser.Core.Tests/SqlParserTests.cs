using Parlot;
using SqlParser.Core;
using SqlParser.Core.Statements;
using SqlParser.Core.Syntax;
using System.Linq;
using Xunit;

namespace SqlParser.Tests
{
    public class SqlParserTests
    {
        [Theory]
        [InlineData("12", 12)]
        [InlineData("12.5", 12.5)]
        public void ParseNumberExpression(string text, decimal expected)
        {
            // Arrange
            var parser = new Core.SqlParser();
            var context = new SqlContext(text);
            var result = new ParseResult<SyntaxNode>();

            // Arrange
            parser.Expression.Parse(context, ref result);

            // Assert
            var value = result.Value;
            Assert.Equal(SyntaxKind.NumberToken, value.Kind);
            Assert.Equal(expected, value.Token.Value);
        }

        [Theory]
        [InlineData("TRUE", true)]
        [InlineData("trUE", true)]
        [InlineData("false", false)]
        [InlineData("False", false)]
        public void ParseBooleanExpression(string text, bool expected)
        {
            // Arrange
            var parser = new Core.SqlParser();
            var context = new SqlContext(text);
            var result = new ParseResult<SyntaxNode>();

            // Arrange
            parser.Expression.Parse(context, ref result);

            // Assert
            var value = result.Value;
            Assert.Equal(SyntaxKind.BooleanToken, value.Kind);
            Assert.Equal(expected, value.Token.Value);
        }

        [Theory]
        [InlineData("\"Hisham\"", "Hisham")]
        [InlineData("\'Hisham\'", "Hisham")]
        public void ParseStringExpression(string text, string expected)
        {
            // Arrange
            var parser = new Core.SqlParser();
            var context = new SqlContext(text);
            var result = new ParseResult<SyntaxNode>();

            // Arrange
            parser.Expression.Parse(context, ref result);

            // Assert
            var value = result.Value;
            Assert.Equal(SyntaxKind.StringToken, value.Kind);
            Assert.Equal(expected, value.Token.Value);
        }

        [Theory]
        [InlineData("Name", "Name")]
        [InlineData("Name123", "Name123")]
        [InlineData("_Name", "_Name")]
        [InlineData("First_Name", "First_Name")]
        public void ParseIdentifierExpression(string text, string expected)
        {
            // Arrange
            var parser = new Core.SqlParser();
            var context = new SqlContext(text);
            var result = new ParseResult<SyntaxNode>();

            // Arrange
            parser.Expression.Parse(context, ref result);

            // Assert
            var value = result.Value;
            Assert.Equal(SyntaxKind.IdentifierToken, value.Kind);
            Assert.Equal(expected, value.Token.Value);
        }

        [Theory]
        [InlineData("(120)", 120)]
        [InlineData("(12.3)", 12.3)]
        public void ParseGroupExpression(string text, decimal expected)
        {
            // Arrange
            var parser = new Core.SqlParser();
            var context = new SqlContext(text);
            var result = new ParseResult<SyntaxNode>();

            // Arrange
            parser.Expression.Parse(context, ref result);

            // Assert
            var value = result.Value;
            Assert.Equal(SyntaxKind.NumberToken, value.Kind);
            Assert.Equal(expected, value.Token.Value);
        }

        [Theory]
        [InlineData("SELECT * FROM People", 1, "SelectStatement")]
        [InlineData("SELECT FirstName, LastName FROM People", 1, "SelectStatement")]
        [InlineData("SELECT FROM People", 0, null)]
        [InlineData("INSERT INTO People (FirstName, LastName) VALUES ('Jon', 'Doe')", 1, "InsertStatement")]
        [InlineData("INSERT INTO People VALUES ('Jon', 'Doe')", 1, "InsertStatement")]
        [InlineData("INSERT INTO VALUES ('Jon', 'Doe')", 0, null)]
        [InlineData("DELETE FROM People", 1, "DeleteStatement")]
        [InlineData("DELETE * FROM People", 0, null)]
        [InlineData("UPDATE People SET FirstName='Jon', Age=32", 1, "UpdateStatement")]
        public void ParseSqlStatement(string commandText, int statementsNo, string expectedStatmentType)
        {
            // Arrange
            var parser = new Core.SqlParser();

            // Act
            var syntaxTree = parser.Parse(commandText);

            // Assert
            Assert.True(syntaxTree.Statements.Count() == statementsNo);
            Assert.Equal(expectedStatmentType, syntaxTree.Statements.SingleOrDefault()?.GetType()?.Name ?? null);
        }

        [Theory]
        [InlineData("SELECT * FROM People;SELECT * FROM Customers", 2)]
        [InlineData("SELECT * FROM People;SELECT * FROM Customers;SELECT * FROM Accounts", 3)]
        public void ParseMultipleSqlStatements(string commandText, int statementsNo)
        {
            // Arrange
            var parser = new Core.SqlParser();

            // Act
            var syntaxTree = parser.Parse(commandText);

            // Assert
            Assert.True(syntaxTree.Statements.Count() == statementsNo);
            Assert.True(syntaxTree.Statements.All(s => s.GetType().Name == nameof(SelectStatement)));
        }
    }
}
