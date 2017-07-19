using Datalust.Piggy.Syntax;
using Xunit;

namespace Datalust.Piggy.Tests.Syntax
{
    public class ChangeScriptDirectiveParserTests
    {
        [Fact]
        public void ByDefaultScriptsAreTransacted()
        {
            var directives = ChangeScriptDirectiveParser.Parse("INSERT INTO foo (id) VALUES (1)");
            Assert.True(directives.IsTransacted);
        }

        [Fact]
        public void DirectiveCanSpecifyNoTransaction()
        {
            var directives = ChangeScriptDirectiveParser.Parse("-- PIGGY NO TRANSACTION\nINSERT INTO foo (id) VALUES (1)");
            Assert.False(directives.IsTransacted);
        }
    }
}
