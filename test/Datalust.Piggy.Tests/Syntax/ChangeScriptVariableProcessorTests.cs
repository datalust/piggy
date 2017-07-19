using System.Collections.Generic;
using Datalust.Piggy.Syntax;
using Xunit;

namespace Datalust.Piggy.Tests.Syntax
{
    public class ChangeScriptVariableProcessorTests
    {
        [Fact]
        public void ConfiguredVariablesAreReplaced()
        {
            var variables = new Dictionary<string, string>
            {
                ["one"] = "1",
                ["two"] = "2"
            };

            const string content = "$$one$-$two$-$two$-$three$";
            var result = ChangeScriptVariableProcessor.InsertVariables(content, variables);
            Assert.Equal("$1-2-2-$three$", result);
        }
    }
}
