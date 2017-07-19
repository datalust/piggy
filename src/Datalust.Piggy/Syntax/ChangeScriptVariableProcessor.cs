using System.Collections.Generic;

namespace Datalust.Piggy.Syntax
{
    static class ChangeScriptVariableProcessor
    {
        public static string InsertVariables(string content, IReadOnlyDictionary<string, string> variables)
        {
            var result = content;

            foreach (var variable in variables)
            {
                var token = $"${variable.Key}$";
                result = result.Replace(token, variable.Value);
            }

            return result;
        }
    }
}
