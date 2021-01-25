using Parlot;
using SqlParser.Core;
using SqlParser.Core.Statements;
using SqlParser.Core.Syntax;
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
            Assert.Equal(expectedTableName, statement.TableName);
            Assert.Equal(expectedColumnNames, statement.ColumnNames);
            Assert.True(expectedValues.SequenceEqual(statement.Values, new SequenceComparer()));
        }

        [Fact]
        public void GetInsertStatementNodesInfo()
        {
            // Arrange
            var sql = "Insert Into People (FirstName, LastName, Age) VALUES ('Jon', 'Doe', 32)";
            var context = new SqlContext(sql);
            var result = new ParseResult<Statement>();

            // Act
            InsertStatement.Statement.Parse(context, ref result);

            // Assert
            var statement = result.Value as InsertStatement;
            Assert.Equal(2, statement.Nodes.Count());

            var insertIntoClause = statement.Nodes[0];
            Assert.Equal(SyntaxKind.InsertIntoClause, insertIntoClause.Token.Kind);
            Assert.Equal(SyntaxKind.InsertKeyword, insertIntoClause.ChildNodes[0].Token.Kind);
            Assert.Equal("INSERT", insertIntoClause.ChildNodes[0].Token.Value);
            Assert.Equal(SyntaxKind.IntoKeyword, insertIntoClause.ChildNodes[1].Token.Kind);
            Assert.Equal("INTO", insertIntoClause.ChildNodes[1].Token.Value);
            Assert.Equal(SyntaxKind.IdentifierToken, insertIntoClause.ChildNodes[2].Token.Kind);
            Assert.Equal("People", insertIntoClause.ChildNodes[2].Token.Value);
            Assert.Equal(SyntaxKind.OpenParenthesisToken, insertIntoClause.ChildNodes[3].Token.Kind);
            Assert.Equal(SyntaxKind.IdentifierToken, insertIntoClause.ChildNodes[4].Token.Kind);
            Assert.Equal("FirstName", insertIntoClause.ChildNodes[4].Token.Value);
            Assert.Equal(SyntaxKind.CommaToken, insertIntoClause.ChildNodes[5].Token.Kind);
            Assert.Equal(SyntaxKind.IdentifierToken, insertIntoClause.ChildNodes[6].Token.Kind);
            Assert.Equal("LastName", insertIntoClause.ChildNodes[6].Token.Value);
            Assert.Equal(SyntaxKind.CommaToken, insertIntoClause.ChildNodes[7].Token.Kind);
            Assert.Equal(SyntaxKind.IdentifierToken, insertIntoClause.ChildNodes[8].Token.Kind);
            Assert.Equal("Age", insertIntoClause.ChildNodes[8].Token.Value);
            Assert.Equal(SyntaxKind.CloseParenthesisToken, insertIntoClause.ChildNodes[9].Token.Kind);

            var valuesClause = statement.Nodes[1];
            Assert.Equal(SyntaxKind.ValuesClause, valuesClause.Token.Kind);
            Assert.Equal(SyntaxKind.ValuesKeyword, valuesClause.ChildNodes[0].Token.Kind);
            Assert.Equal("VALUES", valuesClause.ChildNodes[0].Token.Value);
            Assert.Equal(SyntaxKind.OpenParenthesisToken, valuesClause.ChildNodes[1].Token.Kind);
            Assert.Equal(SyntaxKind.StringToken, valuesClause.ChildNodes[2].Token.Kind);
            Assert.Equal("Jon", valuesClause.ChildNodes[2].Token.Value);
            Assert.Equal(SyntaxKind.CommaToken, valuesClause.ChildNodes[3].Token.Kind);
            Assert.Equal(SyntaxKind.StringToken, valuesClause.ChildNodes[4].Token.Kind);
            Assert.Equal("Doe", valuesClause.ChildNodes[4].Token.Value);
            Assert.Equal(SyntaxKind.CommaToken, valuesClause.ChildNodes[5].Token.Kind);
            Assert.Equal(SyntaxKind.NumberToken, valuesClause.ChildNodes[6].Token.Kind);
            Assert.Equal(32M, valuesClause.ChildNodes[6].Token.Value);
            Assert.Equal(SyntaxKind.CloseParenthesisToken, valuesClause.ChildNodes[7].Token.Kind);
        }

        [Fact]
        public void GetInsertStatementNodesInfo_WithoutColumns()
        {
            // Arrange
            var sql = "Insert Into People VALUES ('Jon', 'Doe', 32)";
            var context = new SqlContext(sql);
            var result = new ParseResult<Statement>();

            // Act
            InsertStatement.Statement.Parse(context, ref result);

            // Assert
            var statement = result.Value as InsertStatement;
            Assert.Equal(2, statement.Nodes.Count());

            var insertIntoClause = statement.Nodes[0];
            Assert.Equal(SyntaxKind.InsertIntoClause, insertIntoClause.Token.Kind);
            Assert.Equal(SyntaxKind.InsertKeyword, insertIntoClause.ChildNodes[0].Token.Kind);
            Assert.Equal("INSERT", insertIntoClause.ChildNodes[0].Token.Value);
            Assert.Equal(SyntaxKind.IntoKeyword, insertIntoClause.ChildNodes[1].Token.Kind);
            Assert.Equal("INTO", insertIntoClause.ChildNodes[1].Token.Value);
            Assert.Equal(SyntaxKind.IdentifierToken, insertIntoClause.ChildNodes[2].Token.Kind);
            Assert.Equal("People", insertIntoClause.ChildNodes[2].Token.Value);

            var valuesClause = statement.Nodes[1];
            Assert.Equal(SyntaxKind.ValuesClause, valuesClause.Token.Kind);
            Assert.Equal(SyntaxKind.ValuesKeyword, valuesClause.ChildNodes[0].Token.Kind);
            Assert.Equal("VALUES", valuesClause.ChildNodes[0].Token.Value);
            Assert.Equal(SyntaxKind.OpenParenthesisToken, valuesClause.ChildNodes[1].Token.Kind);
            Assert.Equal(SyntaxKind.StringToken, valuesClause.ChildNodes[2].Token.Kind);
            Assert.Equal("Jon", valuesClause.ChildNodes[2].Token.Value);
            Assert.Equal(SyntaxKind.CommaToken, valuesClause.ChildNodes[3].Token.Kind);
            Assert.Equal(SyntaxKind.StringToken, valuesClause.ChildNodes[4].Token.Kind);
            Assert.Equal("Doe", valuesClause.ChildNodes[4].Token.Value);
            Assert.Equal(SyntaxKind.CommaToken, valuesClause.ChildNodes[5].Token.Kind);
            Assert.Equal(SyntaxKind.NumberToken, valuesClause.ChildNodes[6].Token.Kind);
            Assert.Equal(32M, valuesClause.ChildNodes[6].Token.Value);
            Assert.Equal(SyntaxKind.CloseParenthesisToken, valuesClause.ChildNodes[7].Token.Kind);
        }
    }
}
