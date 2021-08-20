using System;
using Datalust.Piggy.Cli.Features;
using Datalust.Piggy.Database;
using Datalust.Piggy.History;
using Serilog;

namespace Datalust.Piggy.Cli.Commands
{
    [Command("log", "List change scripts that have been applied to a database")]
    class LogCommand : Command
    {
        readonly UsernamePasswordFeature _usernamePasswordFeature;
        readonly DatabaseFeature _databaseFeature;
        readonly LoggingFeature _loggingFeature;

        public LogCommand()
        {
            _databaseFeature = Enable<DatabaseFeature>();
            _usernamePasswordFeature = Enable<UsernamePasswordFeature>();
            _loggingFeature = Enable<LoggingFeature>();
        }

        protected override int Run()
        {
            _loggingFeature.Configure();
            
            try
            {
                using var connection = DatabaseConnector.Connect(_databaseFeature.Host!, _databaseFeature.Database!,
                    _usernamePasswordFeature.Username!, _usernamePasswordFeature.Password!, false);
                foreach (var applied in AppliedChangeScriptLog.GetAppliedChangeScripts(connection))
                {
                    Console.WriteLine($"{applied.AppliedAt:o} {applied.AppliedBy} {applied.ScriptFile}");
                }

                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Could not list change scripts");
                return -1;
            }
        }
    }
}
