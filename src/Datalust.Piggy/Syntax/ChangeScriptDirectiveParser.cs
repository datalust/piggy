using System.IO;

namespace Datalust.Piggy.Syntax
{
    static class ChangeScriptDirectiveParser
    {
        public static ChangeScriptDirectives Parse(string scriptContent)
        {
            var firstLine = new StringReader(scriptContent).ReadLine();
            var result = new ChangeScriptDirectives
            {
                IsTransacted = !(firstLine?.Trim().Equals("-- PIGGY NO TRANSACTION") ?? false)
            };
            return result;
        }
    }
}
