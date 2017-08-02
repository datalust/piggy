using System.Text;
using Datalust.Piggy.Database;
using Datalust.Piggy.History;
using Datalust.Piggy.Status;
using Npgsql;
using Serilog;

namespace Datalust.Piggy.Baseline
{
    class BaselineSession
    {
        public static void BaselineDatabase(string host, string database, string username, string password, string scriptRoot)
        {
            using (var connection = DatabaseConnector.Connect(host, database, username, password, false))
            {
                var scripts = DatabaseStatus.GetPendingScripts(connection, scriptRoot);

                Log.Information("Found {Count} script files without change log records", scripts.Length);

                if (scripts.Length != 0)
                {
                    var commandText = new StringBuilder();
                    commandText.AppendLine("START TRANSACTION ISOLATION LEVEL REPEATABLE READ;");
                    commandText.AppendLine(AppliedChangeScriptLog.ChangesTableCreateScript);

                    foreach (var script in scripts)
                    {
                        Log.Information("Recording {FullPath} as {ScriptFile}", script.FullPath, script.RelativeName);
                        commandText.AppendLine(AppliedChangeScriptLog.CreateApplyLogScriptFor(script));
                    }

                    commandText.AppendLine("COMMIT TRANSACTION;");

                    Log.Information("Writing change log");
                    using (var command = new NpgsqlCommand(commandText.ToString(), connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }

                Log.Information("Done");
            }
        }
    }
}
