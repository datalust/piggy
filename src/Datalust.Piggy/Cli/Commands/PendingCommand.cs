using System;
using Datalust.Piggy.Cli.Features;
using Datalust.Piggy.Database;
using Datalust.Piggy.Status;
using Serilog;

namespace Datalust.Piggy.Cli.Commands
{
    [Command("pending", "Determine which scripts will be run in an update")]
    class PendingCommand : Command
    {
        readonly UsernamePasswordFeature _usernamePasswordFeature;
        readonly DatabaseFeature _databaseFeature;
        readonly LoggingFeature _loggingFeature;
        readonly ScriptRootFeature _scriptRootFeature;

        public PendingCommand()
        {
            _databaseFeature = Enable<DatabaseFeature>();
            _usernamePasswordFeature = Enable<UsernamePasswordFeature>();
            _scriptRootFeature = Enable<ScriptRootFeature>();
            _loggingFeature = Enable<LoggingFeature>();
        }

        protected override int Run()
        {
            _loggingFeature.Configure();

            try
            {
                using (var connection = DatabaseConnector.Connect(_databaseFeature.Host!, _databaseFeature.Database!,
                    _usernamePasswordFeature.Username!, _usernamePasswordFeature.Password!, false))
                {
                    foreach (var pending in DatabaseStatus.GetPendingScripts(connection, _scriptRootFeature.ScriptRoot!))
                    {
                        Console.WriteLine($"{pending.RelativeName}");
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Could not determine pending change scripts");
                return -1;
            }
        }
    }
}
