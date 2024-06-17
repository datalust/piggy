using System;
using System.Collections.Generic;
using System.Linq;
using Datalust.Piggy.Filesystem;
using Datalust.Piggy.History;
using Npgsql;

namespace Datalust.Piggy.Status
{
    static class DatabaseStatus
    {
        public static ChangeScriptFile[] GetPendingScripts(NpgsqlConnection connection, string scriptRoot)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));
            if (scriptRoot == null) throw new ArgumentNullException(nameof(scriptRoot));

            var changes = AppliedChangeScriptLog.GetAppliedChangeScripts(connection);

            var applied = new HashSet<string>(changes.Select(m => m.ScriptFile!));
            return ChangeScriptFileEnumerator.EnumerateInOrder(scriptRoot)
                .Where(s => !applied.Contains(s.RelativeName))
                .ToArray();
        }
    }
}
