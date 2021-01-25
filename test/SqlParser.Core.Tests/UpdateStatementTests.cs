using Parlot;
using SqlParser.Core;
using SqlParser.Core.Statements;
using SqlParser.Core.Syntax;
using System.Linq;
using Xunit;

namespace SqlParser.Tests
{
    public class UpdateStatementTests
    {
        [Theory]
        [InlineData("Update People Set Age = 32", "People", new[] { "Age" }, new object[] { 32 })]
        [InlineData("Update People Set FirstName = 'Jon', LastName = 'Doe'", "People", new [] { "FirstName", "LastName" }, new object[] { "Jon", "Doe" })]
        public void ParseUpdateStatement(string text, string expectedTableName, string[] expectedColumnNames, object[] expectedValues)
        {
            // Arrange
            var context = new SqlContext(text);
            var result = new ParseResult<Statement>();

            // Act
            UpdateStatement.Statement.Parse(context, ref result);

            // Assert
            var statement = result.Value as UpdateStatement;
            Assert.Equal(expectedTableName, statement.TableName);
            Assert.Equal(expectedColumnNames, statement.ColumnNames);
            Assert.True(expectedValues.SequenceEqual(statement.Values, new SequenceComparer()));
        }

        [Fact]
        public void GetUpdateStatementNodesInfo()
        {
            // Arrange
            var sql = "Update People Set FirstName = 'Jon', Age = 32";
            var context = new SqlContext(sql);
            var result = new ParseResult<Statement>();

            // Act
            UpdateStatement.Statement.Parse(context, ref result);

            // Assert
            var statement = result.Value as UpdateStatement;
            Assert.Equal(2, statement.Nodes.Count());

            var updateClause = statement.Nodes[0];
            Assert.Equal(SyntaxKind.UpdateClause, updateClause.Token.Kind);
            Assert.Equal(SyntaxKind.UpdateKeyword, updateClause.ChildNodes[0].Token.Kind);
            Assert.Equal("UPDATE", updateClause.ChildNodes[0].Token.Value);
            Assert.Equal(SyntaxKind.IdentifierToken, updateClause.ChildNodes[1].Token.Kind);
            Assert.Equal("People", updateClause.ChildNodes[1].Token.Value);

            var setClause = statement.Nodes[1];
            Assert.Equal(SyntaxKind.SetClause, setClause.Token.Kind);
            Assert.Equal(SyntaxKind.SetKeyword, setClause.ChildNodes[0].Token.Kind);
            Assert.Equal("SET", setClause.ChildNodes[0].Token.Value);
            Assert.Equal(SyntaxKind.IdentifierToken, setClause.ChildNodes[1].Token.Kind);
            Assert.Equal("FirstName", setClause.ChildNodes[1].Token.Value);
            Assert.Equal(SyntaxKind.EqualsToken, setClause.ChildNodes[2].Token.Kind);
            Assert.Equal(SyntaxKind.StringToken, setClause.ChildNodes[3].Token.Kind);
            Assert.Equal("Jon", setClause.ChildNodes[3].Token.Value);
            Assert.Equal(SyntaxKind.CommaToken, setClause.ChildNodes[4].Token.Kind);
            Assert.Equal(SyntaxKind.IdentifierToken, setClause.ChildNodes[5].Token.Kind);
            Assert.Equal("Age", setClause.ChildNodes[5].Token.Value);
            Assert.Equal(SyntaxKind.EqualsToken, setClause.ChildNodes[6].Token.Kind);
            Assert.Equal(SyntaxKind.NumberToken, setClause.ChildNodes[7].Token.Kind);
            Assert.Equal(32M, setClause.ChildNodes[7].Token.Value);
        }
    }
}
