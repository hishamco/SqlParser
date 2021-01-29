using Parlot;
using SqlParser.Core;
using SqlParser.Core.Statements;
using SqlParser.Core.Syntax;
using System.Linq;
using Xunit;

namespace SqlParser.Tests
{
    public class SelectStatementTests
    {
        [Theory]
        [InlineData("Select * From People", new[] { "People" }, new[] { "*" })]
        [InlineData("Select FirstName From People", new[] { "People" }, new[] { "FirstName" })]
        [InlineData("Select FirstName, LastName From People", new[] { "People" }, new[] { "FirstName", "LastName" })]
        [InlineData("select * from People, Contacts", new[] { "People", "Contacts" }, new[] { "*" })]
        [InlineData("select People.FirstName, Contatcs.Address from People, Contacts", new[] { "People", "Contacts" }, new[] { "FirstName", "Address" })]
        public void ParseSelectStatement(string text, string[] expectedTableNames, string[] expectedColumnNames)
        {
            // Arrange
            var context = new SqlContext(text);
            var result = new ParseResult<Statement>();

            // Act
            SelectStatement.Statement.Parse(context, ref result);

            // Assert
            var statement = result.Value as SelectStatement;
            Assert.Equal(expectedTableNames, statement.TableNames.ToArray());
            Assert.Equal(expectedColumnNames, statement.ColumnNames.ToArray());
        }

        [Theory]
        [InlineData("Select FirstName As Name From People", new string[] { "Name" }, new string[] { })]
        [InlineData("Select People.FirstName As Name From People", new string[] { "Name" }, new string[] { })]
        [InlineData("Select FirstName As 'First Name', LastName As 'Sure Name' From People", new string[] { "First Name", "Sure Name" }, new string[] { })]
        [InlineData("Select * From People As Persons", new string[] { }, new string[] { "Persons" })]
        [InlineData("Select FirstName As 'First Name', LastName As 'Sure Name' From People As Persons", new string[] { "First Name", "Sure Name" }, new string[] { "Persons" })]
        public void ParseColumnAndTableAliases(string text, string[] expectedColumnAliases, string[] expectedTableAliases)
        { 
            // Arrange
            var context = new SqlContext(text);
            var result = new ParseResult<Statement>();

            // Act
            SelectStatement.Statement.Parse(context, ref result);

            // Assert
            var statement = result.Value as SelectStatement;
            Assert.Equal(expectedColumnAliases, statement.ColumnAliases.ToArray());
            Assert.Equal(expectedTableAliases, statement.TableAliases.ToArray());
        }

        [Theory]
        [InlineData("Select Distinct * From People", new string[] { "People" }, new string[] { "*" })]
        [InlineData("Select Distinct FirstName, LastName From People", new string[] { "People" }, new string[] { "FirstName", "LastName" })]
        public void ParseDistinct(string text, string[] expectedTableNames, string[] expectedColumnNames)
        {
            // Arrange
            var context = new SqlContext(text);
            var result = new ParseResult<Statement>();

            // Act
            SelectStatement.Statement.Parse(context, ref result);

            // Assert
            var statement = result.Value as SelectStatement;
            Assert.Equal(SyntaxKind.DistinctKeyword, statement.Nodes[0].ChildNodes[1].Kind);
            Assert.Equal(expectedTableNames, statement.TableNames.ToArray());
            Assert.Equal(expectedColumnNames, statement.ColumnNames.ToArray());
        }

        [Fact]
        public void GetSelectStatementNodesInfo()
        {
            // Arrange
            var sql = "Select Persons.FirstName, LastName As 'Sure Name' From People As Persons";
            var context = new SqlContext(sql);
            var result = new ParseResult<Statement>();

            // Act
            SelectStatement.Statement.Parse(context, ref result);

            // Assert
            var statement = result.Value as SelectStatement;
            Assert.Equal(2, statement.Nodes.Count());

            var selectClause = statement.Nodes[0];
            Assert.Equal(SyntaxKind.SelectClause, selectClause.Token.Kind);
            Assert.Equal(SyntaxKind.SelectKeyword, selectClause.ChildNodes[0].Token.Kind);
            Assert.Equal("SELECT", selectClause.ChildNodes[0].Token.Value);
            Assert.Equal(SyntaxKind.IdentifierToken, selectClause.ChildNodes[1].ChildNodes[0].Token.Kind);
            Assert.Equal("Persons", selectClause.ChildNodes[1].ChildNodes[0].Token.Value);
            Assert.Equal(SyntaxKind.DotToken, selectClause.ChildNodes[1].ChildNodes[1].Token.Kind);
            Assert.Equal(SyntaxKind.IdentifierToken, selectClause.ChildNodes[1].ChildNodes[2].Token.Kind);
            Assert.Equal("FirstName", selectClause.ChildNodes[1].ChildNodes[2].Token.Value);
            Assert.Equal(SyntaxKind.CommaToken, selectClause.ChildNodes[2].Token.Kind);
            Assert.Equal(SyntaxKind.IdentifierToken, selectClause.ChildNodes[3].ChildNodes[0].Token.Kind);
            Assert.Equal("LastName", selectClause.ChildNodes[3].ChildNodes[0].Token.Value);
            Assert.Equal(SyntaxKind.AsKeyword, selectClause.ChildNodes[3].ChildNodes[1].Token.Kind);
            Assert.Equal(SyntaxKind.StringToken, selectClause.ChildNodes[3].ChildNodes[2].Token.Kind);
            Assert.Equal("Sure Name", selectClause.ChildNodes[3].ChildNodes[2].Token.Value);

            var fromClause = statement.Nodes[1];
            Assert.Equal(SyntaxKind.FromClause, fromClause.Token.Kind);
            Assert.Equal(SyntaxKind.FromKeyword, fromClause.ChildNodes[0].Token.Kind);
            Assert.Equal("FROM", fromClause.ChildNodes[0].Token.Value);
            Assert.Equal(SyntaxKind.IdentifierToken, fromClause.ChildNodes[1].ChildNodes[0].Token.Kind);
            Assert.Equal("People", fromClause.ChildNodes[1].ChildNodes[0].Token.Value);
            Assert.Equal(SyntaxKind.AsKeyword, fromClause.ChildNodes[1].ChildNodes[1].Token.Kind);
            Assert.Equal(SyntaxKind.IdentifierToken, fromClause.ChildNodes[1].ChildNodes[2].Token.Kind);
            Assert.Equal("Persons", fromClause.ChildNodes[1].ChildNodes[2].Token.Value);
        }
    }
}
