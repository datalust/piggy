using System.Collections.Generic;
using System.IO;
using System.Text;
using Datalust.Piggy.Filesystem;
using Datalust.Piggy.History;
using Datalust.Piggy.Status;
using Datalust.Piggy.Syntax;
using Npgsql;
using Serilog;

namespace Datalust.Piggy.Update
{
    /// <summary>
    /// Applies updates to a target database.
    /// </summary>
    public static class UpdateSession
    {
        /// <summary>
        /// Apply change scripts from a folder hierarchy.
        /// </summary>
        /// <param name="connection">The database connection to use.</param>
        /// <param name="scriptRoot">A root filesystem folder under which change scripts are stored.</param>
        /// <param name="variables">A set of variables to replace within change scripts.</param>
        public static void ApplyChangeScripts(NpgsqlConnection connection, string scriptRoot, IReadOnlyDictionary<string, string> variables)
        {
            var scripts = DatabaseStatus.GetPendingScripts(connection, scriptRoot);

            Log.Information("Found {Count} new script files to apply", scripts.Length);

            if (scripts.Length != 0)
            {
                Log.Information("Ensuring the change log table exists");
                using (var command = new NpgsqlCommand(AppliedChangeScriptLog.ChangesTableCreateScript, connection))
                    command.ExecuteNonQuery();
            }

            foreach (var script in scripts)
            {
                Log.Information("Applying {FullPath} as {ScriptFile}", script.FullPath, script.RelativeName);
                ApplyChangeScript(connection, script, variables);
            }

            Log.Information("Done");
        }

        static void ApplyChangeScript(NpgsqlConnection connection, ChangeScriptFile script, IReadOnlyDictionary<string, string> variables)
        {
            var content = File.ReadAllText(script.FullPath);
            var directives = ChangeScriptDirectiveParser.Parse(content);

            var commandText = new StringBuilder();
            if (directives.IsTransacted)
                commandText.AppendLine("START TRANSACTION ISOLATION LEVEL REPEATABLE READ;");

            commandText.AppendLine(ChangeScriptVariableProcessor.InsertVariables(content, variables));
            commandText.AppendLine(";");

            commandText.AppendLine(AppliedChangeScriptLog.CreateApplyLogScriptFor(script));
            
            if (directives.IsTransacted)
                commandText.AppendLine("COMMIT TRANSACTION;");

            using (var command = new NpgsqlCommand(commandText.ToString(), connection))
            {
                command.ExecuteNonQuery();
            }
        }
    }
}
