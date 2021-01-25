using Parlot;
using SqlParser.Core;
using SqlParser.Core.Statements;
using SqlParser.Core.Syntax;
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
            Assert.Equal(expected, statement.TableName);
        }

        [Fact]
        public void GetDeleteStatementNodesInfo()
        {
            // Arrange
            var sql = "DELETE FROM Customers";
            var context = new SqlContext(sql);
            var result = new ParseResult<Statement>();

            // Act
            DeleteStatement.Statement.Parse(context, ref result);

            // Assert
            var statement = result.Value as DeleteStatement;
            Assert.Equal(2, statement.Nodes.Count());

            var deleteClause = statement.Nodes[0];
            Assert.Equal(SyntaxKind.DeleteClause, deleteClause.Token.Kind);
            Assert.Equal(SyntaxKind.DeleteKeyword, deleteClause.ChildNodes[0].Token.Kind);
            Assert.Equal("DELETE", deleteClause.ChildNodes[0].Token.Value);

            var fromClause = statement.Nodes[1];
            Assert.Equal(SyntaxKind.FromClause, fromClause.Token.Kind);
            Assert.Equal(SyntaxKind.FromKeyword, fromClause.ChildNodes[0].Token.Kind);
            Assert.Equal("FROM", fromClause.ChildNodes[0].Token.Value);
            Assert.Equal(SyntaxKind.IdentifierToken, fromClause.ChildNodes[1].Token.Kind);
            Assert.Equal("Customers", fromClause.ChildNodes[1].Token.Value);
        }
    }
}
