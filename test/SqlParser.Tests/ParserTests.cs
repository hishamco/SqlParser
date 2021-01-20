﻿using Parlot;
using SqlParser.Expressions;
using SqlParser.Statements;
using SqlParser.Values;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SqlParser.Tests
{
    public class ParserTests
    {
        [Theory]
        [InlineData("12", 12)]
        [InlineData("12.5", 12.5)]
        public async Task ParseNumberExpression(string text, decimal expected)
        {
            // Arrange
            var parser = new Parser();
            var context = new SqlContext(text);
            var result = new ParseResult<Expression>();

            // Arrange
            parser.Expression.Parse(context, ref result);

            var value = await (result.Value as NumericExpression).EvaluateAsync();

            // Assert
            Assert.Equal(new NumberValue(expected), value);
        }

        [Theory]
        [InlineData("TRUE", true)]
        [InlineData("trUE", true)]
        [InlineData("false", false)]
        [InlineData("False", false)]
        public async Task ParseBooleanExpression(string text, bool expected)
        {
            // Arrange
            var parser = new Parser();
            var context = new SqlContext(text);
            var result = new ParseResult<Expression>();

            // Arrange
            parser.Expression.Parse(context, ref result);

            var value = await (result.Value as BooleanExpression).EvaluateAsync();

            // Assert
            Assert.Equal(new BooleanValue(expected), value);
        }

        [Theory]
        [InlineData("\"Hisham\"", "Hisham")]
        [InlineData("\'Hisham\'", "Hisham")]
        public async Task ParseStringExpression(string text, string expected)
        {
            // Arrange
            var parser = new Parser();
            var context = new SqlContext(text);
            var result = new ParseResult<Expression>();

            // Arrange
            parser.Expression.Parse(context, ref result);

            var value = await (result.Value as LiteralExpression).EvaluateAsync();

            // Assert
            Assert.Equal(new StringValue(expected), value);
        }

        [Theory]
        [InlineData("Name", "Name")]
        [InlineData("Name123", "Name123")]
        [InlineData("_Name", "_Name")]
        [InlineData("First_Name", "First_Name")]
        public async Task ParseIdentifierExpression(string text, string expected)
        {
            // Arrange
            var parser = new Parser();
            var context = new SqlContext(text);
            var result = new ParseResult<Expression>();

            // Arrange
            parser.Expression.Parse(context, ref result);

            var value = await(result.Value as LiteralExpression).EvaluateAsync();

            // Assert
            Assert.Equal(new StringValue(expected), value);
        }

        [Theory]
        [InlineData("(120)", 120)]
        [InlineData("(12.3)", 12.3)]
        public async Task ParseGroupExpression(string text, decimal expected)
        {
            // Arrange
            var parser = new Parser();
            var context = new SqlContext(text);
            var result = new ParseResult<Expression>();

            // Arrange
            parser.Expression.Parse(context, ref result);

            var value = await (result.Value as NumericExpression).EvaluateAsync();

            // Assert
            Assert.Equal(new NumberValue(expected), value);
        }

        [Theory]
        [InlineData("5+2", 7)]
        [InlineData("5-2", 3)]
        [InlineData("5*2", 10)]
        [InlineData("-5*2", -10)]
        [InlineData("5/2", 2.5)]
        [InlineData("5%2", 1)]
        [InlineData("12*3-6+70", 100)]
        [InlineData("150+50-5*5", 175)]
        public async Task EvaluateArithmaticExpression(string text, decimal expected)
        {
            // Arrange
            var parser = new Parser();
            var context = new SqlContext(text);
            var result = new ParseResult<Expression>();

            // Arrange
            parser.Expression.Parse(context, ref result);

            var value = await(result.Value as BinaryExpression).EvaluateAsync();

            // Assert
            Assert.Equal(new NumberValue(expected), value);
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
            var parser = new Parser();

            // Act
            var statements = parser.Parse(commandText);

            // Assert
            Assert.True(statements.Count() == statementsNo);
            Assert.Equal(expectedStatmentType, statements.SingleOrDefault()?.GetType()?.Name ?? null);
        }

        [Theory]
        [InlineData("SELECT * FROM People;SELECT * FROM Customers", 2)]
        [InlineData("SELECT * FROM People;SELECT * FROM Customers;SELECT * FROM Accounts", 3)]
        public void ParseMultipleSqlStatements(string commandText, int statementsNo)
        {
            // Arrange
            var parser = new Parser();

            // Act
            var statements = parser.Parse(commandText);

            // Assert
            Assert.True(statements.Count() == statementsNo);
            Assert.True(statements.All(s => s.GetType().Name == nameof(SelectStatement)));
        }
    }
}
