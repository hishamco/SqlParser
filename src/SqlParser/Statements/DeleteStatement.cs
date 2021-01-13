using Parlot;
using Parlot.Fluent;
using SqlParser.Expressions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Parlot.Fluent.Parsers;

namespace SqlParser.Statements
{
    /*
     * deleteStatement ::= DELETE FROM tableName
     * 
     * tableName :: = identifier
     */
    public class DeleteStatement : Statement
    {
        protected static readonly Parser<string> Delete = Terms.Text("DELETE", caseInsensitive: true);
        protected static readonly Parser<string> From = Terms.Text("FROM", caseInsensitive: true);

        private static readonly Deferred<IEnumerable<string>> _tokens = Deferred<IEnumerable<string>>();

        public string TableName { get; private set; }

        static DeleteStatement()
        {
            var identifier = Terms.Identifier()
                .Then<Expression>(e => new IdentifierExpression(e.Buffer));
            var deleteStatement = Delete.And(From).And(identifier);
            _tokens.Parser = deleteStatement.Then<IEnumerable<string>>(e =>
            {
                var result = e.Item3.EvaluateAsync()
                    .GetAwaiter().GetResult()
                    .ToStringValue();

                // TODO: Find a better way to skip a text while constructing a grammar
                var tableName = result.Substring("DELETE FROM ".Length);

                return new List<string> { e.Item1, e.Item2, tableName };
            });
        }

        public DeleteStatement(string commandText) : base(commandText)
        {

        }

        public async override Task TokenizeAsync()
        {
            var context = new SqlParseContext(CommandText);
            var result = new ParseResult<IEnumerable<string>>();

            _tokens.Parse(context, ref result);

            Tokens = result.Value;
            TableName = Tokens.Last();

            await Task.CompletedTask;
        }
    }
}
