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
        [InlineData("Select 1", 2)]
        [InlineData("Select 1 As Alias", 2)]
        [InlineData("Select 1, 2, 3", 6)]
        [InlineData("Select true", 2)]
        [InlineData("Select 'Test'", 2)]
        [InlineData("select 1 + 3", 2)]
        [InlineData("select 1 + 3 As Alias", 2)]
        [InlineData("select Max(*)", 2)]
        [InlineData("select Max(Id)", 2)]
        [InlineData("select Max(Product.Id)", 2)]
        [InlineData("select Max(Product.Id) As ProductId", 2)]
        public void ParseSelectClause(string text, int expectedNodesCount)
        {
            // Arrange
            var context = new SqlContext(text);
            var result = new ParseResult<Statement>();

            // Act
            SelectStatement.Statement.Parse(context, ref result);

            // Assert
            var statement = result.Value as SelectStatement;
            Assert.Equal(expectedNodesCount, statement.Nodes[0].ChildNodes.Count);
        }

        [Fact]
        public void ParseExpressionInSelectClause()
        {
            // Arrange
            var context = new SqlContext("Select 1 + 3 - 2");
            var result = new ParseResult<Statement>();

            // Act
            SelectStatement.Statement.Parse(context, ref result);

            // Assert
            var statement = result.Value as SelectStatement;
            var selectClause = statement.Nodes[0];
            Assert.Equal(SyntaxKind.SelectClause, selectClause.Token.Kind);
            Assert.Equal(SyntaxKind.SelectKeyword, selectClause.ChildNodes[0].Token.Kind);
            Assert.Equal("SELECT", selectClause.ChildNodes[0].Token.Value);
            Assert.Equal(SyntaxKind.MinusToken, selectClause.ChildNodes[1].Token.Kind);
            Assert.Equal(SyntaxKind.PlusToken, selectClause.ChildNodes[1].ChildNodes[0].Token.Kind);
            Assert.Equal(SyntaxKind.NumberToken, selectClause.ChildNodes[1].ChildNodes[1].Token.Kind);
            Assert.Equal(2M, selectClause.ChildNodes[1].ChildNodes[1].Token.Value);
            Assert.Equal(SyntaxKind.NumberToken, selectClause.ChildNodes[1].ChildNodes[0].ChildNodes[0].Token.Kind);
            Assert.Equal(1M, selectClause.ChildNodes[1].ChildNodes[0].ChildNodes[0].Token.Value);
            Assert.Equal(SyntaxKind.NumberToken, selectClause.ChildNodes[1].ChildNodes[0].ChildNodes[1].Token.Kind);
            Assert.Equal(3M, selectClause.ChildNodes[1].ChildNodes[0].ChildNodes[1].Token.Value);
        }

        [Fact]
        public void ParseValuesWithAliasInSelectClause()
        {
            // Arrange
            var context = new SqlContext("Select 1, 2, 3 AS 'Alias'");
            var result = new ParseResult<Statement>();

            // Act
            SelectStatement.Statement.Parse(context, ref result);

            // Assert
            var statement = result.Value as SelectStatement;
            var selectClause = statement.Nodes[0];
            Assert.Equal(SyntaxKind.SelectClause, selectClause.Token.Kind);
            Assert.Equal(SyntaxKind.SelectKeyword, selectClause.ChildNodes[0].Token.Kind);
            Assert.Equal("SELECT", selectClause.ChildNodes[0].Token.Value);
            Assert.Equal(SyntaxKind.NumberToken, selectClause.ChildNodes[1].Token.Kind);
            Assert.Equal(1M, selectClause.ChildNodes[1].Token.Value);
            Assert.Equal(SyntaxKind.CommaToken, selectClause.ChildNodes[2].Token.Kind);
            Assert.Equal(SyntaxKind.NumberToken, selectClause.ChildNodes[3].Token.Kind);
            Assert.Equal(2M, selectClause.ChildNodes[3].Token.Value);
            Assert.Equal(SyntaxKind.CommaToken, selectClause.ChildNodes[4].Token.Kind);
            Assert.Equal(SyntaxKind.AsKeyword, selectClause.ChildNodes[5].Token.Kind);
            Assert.Equal(SyntaxKind.NumberToken, selectClause.ChildNodes[5].ChildNodes[0].Token.Kind);
            Assert.Equal(3M, selectClause.ChildNodes[5].ChildNodes[0].Token.Value);
            Assert.Equal(SyntaxKind.StringToken, selectClause.ChildNodes[5].ChildNodes[1].Token.Kind);
            Assert.Equal("Alias", selectClause.ChildNodes[5].ChildNodes[1].Token.Value);
        }

        [Fact]
        public void ParseFunctionInSelectClause()
        {
            // Arrange
            var context = new SqlContext("Select Count(Id) As 'Total No', Sum(Mark), Count(Products.Id) As Total");
            var result = new ParseResult<Statement>();

            // Act
            SelectStatement.Statement.Parse(context, ref result);

            // Assert
            var statement = result.Value as SelectStatement;
            var selectClause = statement.Nodes[0];
            Assert.Equal(SyntaxKind.SelectClause, selectClause.Token.Kind);
            Assert.Equal(SyntaxKind.SelectKeyword, selectClause.ChildNodes[0].Token.Kind);
            Assert.Equal("SELECT", selectClause.ChildNodes[0].Token.Value);
            Assert.Equal(SyntaxKind.AsKeyword, selectClause.ChildNodes[1].Token.Kind);
            Assert.Equal(SyntaxKind.IdentifierToken, selectClause.ChildNodes[1].ChildNodes[0].Token.Kind);
            Assert.Equal("Count", selectClause.ChildNodes[1].ChildNodes[0].Token.Value);
            Assert.Equal(SyntaxKind.OpenParenthesisToken, selectClause.ChildNodes[1].ChildNodes[0].ChildNodes[0].Token.Kind);
            Assert.Equal(SyntaxKind.IdentifierToken, selectClause.ChildNodes[1].ChildNodes[0].ChildNodes[1].Token.Kind);
            Assert.Equal("Id", selectClause.ChildNodes[1].ChildNodes[0].ChildNodes[1].Token.Value);
            Assert.Equal(SyntaxKind.CloseParenthesisToken, selectClause.ChildNodes[1].ChildNodes[0].ChildNodes[2].Token.Kind);
            Assert.Equal(SyntaxKind.StringToken, selectClause.ChildNodes[1].ChildNodes[1].Token.Kind);
            Assert.Equal("Total No", selectClause.ChildNodes[1].ChildNodes[1].Token.Value);
            Assert.Equal(SyntaxKind.CommaToken, selectClause.ChildNodes[2].Token.Kind);
            Assert.Equal(SyntaxKind.IdentifierToken, selectClause.ChildNodes[3].Token.Kind);
            Assert.Equal("Sum", selectClause.ChildNodes[3].Token.Value);
            Assert.Equal(SyntaxKind.OpenParenthesisToken, selectClause.ChildNodes[3].ChildNodes[0].Token.Kind);
            Assert.Equal(SyntaxKind.IdentifierToken, selectClause.ChildNodes[3].ChildNodes[1].Token.Kind);
            Assert.Equal("Mark", selectClause.ChildNodes[3].ChildNodes[1].Token.Value);
            Assert.Equal(SyntaxKind.CloseParenthesisToken, selectClause.ChildNodes[3].ChildNodes[2].Token.Kind);
            Assert.Equal(SyntaxKind.CommaToken, selectClause.ChildNodes[4].Token.Kind);
            Assert.Equal(SyntaxKind.AsKeyword, selectClause.ChildNodes[5].Token.Kind);
            Assert.Equal(SyntaxKind.IdentifierToken, selectClause.ChildNodes[5].ChildNodes[0].Token.Kind);
            Assert.Equal("Count", selectClause.ChildNodes[5].ChildNodes[0].Token.Value);
            Assert.Equal(SyntaxKind.OpenParenthesisToken, selectClause.ChildNodes[5].ChildNodes[0].ChildNodes[0].Token.Kind);
            Assert.Equal(SyntaxKind.DotToken, selectClause.ChildNodes[5].ChildNodes[0].ChildNodes[1].Token.Kind);
            Assert.Equal(SyntaxKind.IdentifierToken, selectClause.ChildNodes[5].ChildNodes[0].ChildNodes[1].ChildNodes[0].Token.Kind);
            Assert.Equal("Products", selectClause.ChildNodes[5].ChildNodes[0].ChildNodes[1].ChildNodes[0].Token.Value);
            Assert.Equal(SyntaxKind.IdentifierToken, selectClause.ChildNodes[5].ChildNodes[0].ChildNodes[1].ChildNodes[1].Token.Kind);
            Assert.Equal("Id", selectClause.ChildNodes[5].ChildNodes[0].ChildNodes[1].ChildNodes[1].Token.Value);
            Assert.Equal(SyntaxKind.CloseParenthesisToken, selectClause.ChildNodes[5].ChildNodes[0].ChildNodes[2].Token.Kind);
            Assert.Equal(SyntaxKind.IdentifierToken, selectClause.ChildNodes[5].ChildNodes[1].Token.Kind);
            Assert.Equal("Total", selectClause.ChildNodes[5].ChildNodes[1].Token.Value);
        }

        [Theory]
        [InlineData("Select * From Products Where Id")]
        [InlineData("Select * From Products Where Id=5")]
        [InlineData("Select * From Products Where Id = 5")]
        [InlineData("Select * From Products Where Id <> 5")]
        [InlineData("Select * From Products Where Id != 5")]
        [InlineData("Select * From Products Where Id < 5")]
        [InlineData("Select * From Products Where Id > 5")]
        [InlineData("Select * From Products Where Id <= 5")]
        [InlineData("Select * From Products Where Id >= 5")]
        [InlineData("Select * From Products Where Name Like 'M%'")]
        [InlineData("Select * From Products Where Name Not Like 'M%'")]
        [InlineData("Select * From Products Where Not Id >= 5")]
        [InlineData("Select * From Products Where Id >= 5 And Id < 10")]
        [InlineData("Select * From Products Where Id >= 5 Or Id < 10")]
        [InlineData("Select * From Products Where Not Id >= 5 Or Id < 10")]
        [InlineData("Select * From Products Where Name Like 'M%' Or Name Like '%M'")]
        [InlineData("Select * From Products Where Products.Id = 5")]
        [InlineData("Select * From Products Where True")]
        [InlineData("Select * From Products Where False")]
        [InlineData("Select * From Products Where ~Id")]
        [InlineData("Select * From Products Where IsOutOfStock(Id)")]
        [InlineData("Select * From Products Where Id Between 1 And 10")]
        [InlineData("Select * From Products Where Id Not Between 1 And 10")]
        [InlineData("Select * From Products Where Id In (2, 4, 6)")]
        [InlineData("Select * From Products Where Id Not In (2, 4, 6)")]
        [InlineData("Select * From Products Where Id In (Select Id From SpecialProducts)")]
        [InlineData("Select * From Products Where Id Not In (Select Id From SpecialProducts)")]
        [InlineData("Select * From Products Where Id=5 Order By Id")]
        [InlineData("Select * From Products Where Id = 5 Order By Id")]
        [InlineData("Select * From Products Where Id <> 5 Order By Id")]
        [InlineData("Select * From Products Where Id != 5 Order By Id")]
        [InlineData("Select * From Products Where Id < 5 Order By Id")]
        [InlineData("Select * From Products Where Id > 5 Order By Id")]
        [InlineData("Select * From Products Where Id <= 5 Order By Id")]
        [InlineData("Select * From Products Where Id >= 5 Order By Id")]
        [InlineData("Select * From Products Where Name Like 'M%' Or Name Like '%M' Order By Id")]
        public void ParseWhereClause(string text)
        {
            // Arrange
            var context = new SqlContext(text);
            var result = new ParseResult<Statement>();

            // Act
            SelectStatement.Statement.Parse(context, ref result);

            // Assert
            var statement = result.Value as SelectStatement;
            var whereNode = statement.Nodes.Single(n => n.Kind == SyntaxKind.WhereClause);
            Assert.Equal(2, whereNode.ChildNodes.Count);
        }

        [Theory]
        [InlineData("Select 1 As Alias Order By Alias", 2)]
        [InlineData("Select * From Products Order By Id", 2)]
        [InlineData("Select * From Products Order By Products.Id", 2)]
        [InlineData("Select * From Products Order By Id, Name", 4)]
        [InlineData("Select * From Products Order By Id Desc", 3)]
        [InlineData("Select * From Products Order By Products.Id Desc", 3)]
        [InlineData("Select * From Products Order By Id, Name Desc", 5)]
        public void ParseOrderByClause(string text, int expectedNodesCount)
        {
            // Arrange
            var context = new SqlContext(text);
            var result = new ParseResult<Statement>();

            // Act
            SelectStatement.Statement.Parse(context, ref result);

            // Assert
            var statement = result.Value as SelectStatement;
            Assert.Equal(expectedNodesCount, statement.Nodes.Last().ChildNodes.Count);
        }

        [Theory]
        [InlineData("Select * From People", new[] { "People" }, new[] { "*" })]
        [InlineData("Select FirstName From People", new[] { "People" }, new[] { "FirstName" })]
        [InlineData("Select FirstName, LastName From People", new[] { "People" }, new[] { "FirstName", "LastName" })]
        [InlineData("select * from People, Contacts", new[] { "People", "Contacts" }, new[] { "*" })]
        [InlineData("select People.FirstName, Contacts.Address from People, Contacts", new[] { "People", "Contacts" }, new[] { "FirstName", "Address" })]
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
        [InlineData("Select FirstName, LastName, GetFullName(FirstName, LastName) As FullName From People", new string[] { "FullName" }, new string[] { })]
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

        [Theory]
        [InlineData("Select Top(3) * From People", new string[] { "People" }, new string[] { "*" }, 3)]
        [InlineData("Select Top(5) FirstName, LastName From People", new string[] { "People" }, new string[] { "FirstName", "LastName" }, 5)]
        public void ParseTop(string text, string[] expectedTableNames, string[] expectedColumnNames, decimal topCount)
        {
            // Arrange
            var context = new SqlContext(text);
            var result = new ParseResult<Statement>();

            // Act
            SelectStatement.Statement.Parse(context, ref result);

            // Assert
            var statement = result.Value as SelectStatement;
            Assert.Equal(SyntaxKind.TopKeyword, statement.Nodes[0].ChildNodes[1].Kind);
            Assert.Equal(topCount, statement.Nodes[0].ChildNodes[3].Token.Value);
            Assert.Equal(expectedTableNames, statement.TableNames.ToArray());
            Assert.Equal(expectedColumnNames, statement.ColumnNames.ToArray());
        }

        [Fact]
        public void GetSelectStatementNodesInfo()
        {
            // Arrange
            var sql = "Select Distinct Top(3) Persons.FirstName, LastName As 'Sure Name' From People As Persons Where Id >= 6 And Persons.Id < 22 Order By People.Id Desc";
            var context = new SqlContext(sql);
            var result = new ParseResult<Statement>();

            // Act
            SelectStatement.Statement.Parse(context, ref result);

            // Assert
            var statement = result.Value as SelectStatement;
            Assert.Equal(4, statement.Nodes.Count());

            var selectClause = statement.Nodes[0];
            Assert.Equal(SyntaxKind.SelectClause, selectClause.Token.Kind);
            Assert.Equal(SyntaxKind.SelectKeyword, selectClause.ChildNodes[0].Token.Kind);
            Assert.Equal("SELECT", selectClause.ChildNodes[0].Token.Value);
            Assert.Equal(SyntaxKind.DistinctKeyword, selectClause.ChildNodes[1].Token.Kind);
            Assert.Equal("DISTINCT", selectClause.ChildNodes[1].Token.Value);
            Assert.Equal(SyntaxKind.TopKeyword, selectClause.ChildNodes[2].Token.Kind);
            Assert.Equal("TOP", selectClause.ChildNodes[2].Token.Value);
            Assert.Equal(SyntaxKind.OpenParenthesisToken, selectClause.ChildNodes[3].Token.Kind);
            Assert.Equal(SyntaxKind.NumberToken, selectClause.ChildNodes[4].Token.Kind);
            Assert.Equal(3M, selectClause.ChildNodes[4].Token.Value);
            Assert.Equal(SyntaxKind.CloseParenthesisToken, selectClause.ChildNodes[5].Token.Kind);
            Assert.Equal(SyntaxKind.DotToken, selectClause.ChildNodes[6].Token.Kind);
            Assert.Equal(SyntaxKind.IdentifierToken, selectClause.ChildNodes[6].ChildNodes[0].Token.Kind);
            Assert.Equal("Persons", selectClause.ChildNodes[6].ChildNodes[0].Token.Value);
            Assert.Equal(SyntaxKind.IdentifierToken, selectClause.ChildNodes[6].ChildNodes[1].Token.Kind);
            Assert.Equal("FirstName", selectClause.ChildNodes[6].ChildNodes[1].Token.Value);
            Assert.Equal(SyntaxKind.CommaToken, selectClause.ChildNodes[7].Token.Kind);
            Assert.Equal(SyntaxKind.AsKeyword, selectClause.ChildNodes[8].Token.Kind);
            Assert.Equal(SyntaxKind.IdentifierToken, selectClause.ChildNodes[8].ChildNodes[0].Token.Kind);
            Assert.Equal("LastName", selectClause.ChildNodes[8].ChildNodes[0].Token.Value);
            Assert.Equal(SyntaxKind.StringToken, selectClause.ChildNodes[8].ChildNodes[1].Token.Kind);
            Assert.Equal("Sure Name", selectClause.ChildNodes[8].ChildNodes[1].Token.Value);

            var fromClause = statement.Nodes[1];
            Assert.Equal(SyntaxKind.FromClause, fromClause.Token.Kind);
            Assert.Equal(SyntaxKind.FromKeyword, fromClause.ChildNodes[0].Token.Kind);
            Assert.Equal("FROM", fromClause.ChildNodes[0].Token.Value);
            Assert.Equal(SyntaxKind.AsKeyword, fromClause.ChildNodes[1].Token.Kind);
            Assert.Equal(SyntaxKind.IdentifierToken, fromClause.ChildNodes[1].ChildNodes[0].Token.Kind);
            Assert.Equal("People", fromClause.ChildNodes[1].ChildNodes[0].Token.Value);
            Assert.Equal(SyntaxKind.IdentifierToken, fromClause.ChildNodes[1].ChildNodes[1].Token.Kind);
            Assert.Equal("Persons", fromClause.ChildNodes[1].ChildNodes[1].Token.Value);

            var whereClause = statement.Nodes[2];
            Assert.Equal(SyntaxKind.WhereClause, whereClause.Token.Kind);
            Assert.Equal(SyntaxKind.WhereKeyword, whereClause.ChildNodes[0].Token.Kind);
            Assert.Equal("WHERE", whereClause.ChildNodes[0].Token.Value);
            Assert.Equal(SyntaxKind.AndToken, whereClause.ChildNodes[1].Token.Kind);
            Assert.Equal(SyntaxKind.GreaterOrEqualsToken, whereClause.ChildNodes[1].ChildNodes[0].Token.Kind);
            Assert.Equal(SyntaxKind.IdentifierToken, whereClause.ChildNodes[1].ChildNodes[0].ChildNodes[0].Token.Kind);
            Assert.Equal("Id", whereClause.ChildNodes[1].ChildNodes[0].ChildNodes[0].Token.Value);
            Assert.Equal(SyntaxKind.NumberToken, whereClause.ChildNodes[1].ChildNodes[0].ChildNodes[1].Token.Kind);
            Assert.Equal(6M, whereClause.ChildNodes[1].ChildNodes[0].ChildNodes[1].Token.Value);
            Assert.Equal(SyntaxKind.LessToken, whereClause.ChildNodes[1].ChildNodes[1].Token.Kind);
            Assert.Equal(SyntaxKind.DotToken, whereClause.ChildNodes[1].ChildNodes[1].ChildNodes[0].Token.Kind);
            Assert.Equal(SyntaxKind.IdentifierToken, whereClause.ChildNodes[1].ChildNodes[1].ChildNodes[0].ChildNodes[0].Token.Kind);
            Assert.Equal("Persons", whereClause.ChildNodes[1].ChildNodes[1].ChildNodes[0].ChildNodes[0].Token.Value);
            Assert.Equal(SyntaxKind.IdentifierToken, whereClause.ChildNodes[1].ChildNodes[1].ChildNodes[0].ChildNodes[1].Token.Kind);
            Assert.Equal("Id", whereClause.ChildNodes[1].ChildNodes[1].ChildNodes[0].ChildNodes[1].Token.Value);
            Assert.Equal(SyntaxKind.NumberToken, whereClause.ChildNodes[1].ChildNodes[1].ChildNodes[1].Token.Kind);
            Assert.Equal(22M, whereClause.ChildNodes[1].ChildNodes[1].ChildNodes[1].Token.Value);

            var orderByClause = statement.Nodes[3];
            Assert.Equal(SyntaxKind.OrderByClause, orderByClause.Token.Kind);
            Assert.Equal(SyntaxKind.OrderByKeyword, orderByClause.ChildNodes[0].Token.Kind);
            Assert.Equal("ORDER BY", orderByClause.ChildNodes[0].Token.Value);
            Assert.Equal(SyntaxKind.DotToken, orderByClause.ChildNodes[1].Token.Kind);
            Assert.Equal(SyntaxKind.IdentifierToken, orderByClause.ChildNodes[1].ChildNodes[0].Token.Kind);
            Assert.Equal("People", orderByClause.ChildNodes[1].ChildNodes[0].Token.Value);
            Assert.Equal(SyntaxKind.IdentifierToken, orderByClause.ChildNodes[1].ChildNodes[1].Token.Kind);
            Assert.Equal("Id", orderByClause.ChildNodes[1].ChildNodes[1].Token.Value);
            Assert.Equal(SyntaxKind.DescendingKeyword, orderByClause.ChildNodes[2].Token.Kind);
            Assert.Equal("DESC", orderByClause.ChildNodes[2].Token.Value);
        }
    }
}
