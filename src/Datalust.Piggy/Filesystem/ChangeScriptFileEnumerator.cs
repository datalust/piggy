using System;
using System.IO;
using System.Linq;
using Serilog;

namespace Datalust.Piggy.Filesystem
{
    static class ChangeScriptFileEnumerator
    {
        public static ChangeScriptFile[] EnumerateInOrder(string scriptRoot)
        {
            Log.Information("Searching for *.sql change script files in {ScriptRoot}", scriptRoot);

            if (!Directory.Exists(scriptRoot))
                throw new ArgumentException("The script root directory does not exist");

            var root = Path.GetFullPath(scriptRoot);
            if (!root.EndsWith(Path.DirectorySeparatorChar.ToString()))
                root += Path.DirectorySeparatorChar;
            var prefixLength = root.Length;

            return Directory.GetFiles(scriptRoot, "*.sql", SearchOption.AllDirectories)
                .Select(Path.GetFullPath)
                .Select(fp => new ChangeScriptFile(fp, fp.Substring(prefixLength).Replace(Path.DirectorySeparatorChar, '/')))
                .OrderBy(csc => csc.RelativeName)
                .ToArray();
        }
    }
}
