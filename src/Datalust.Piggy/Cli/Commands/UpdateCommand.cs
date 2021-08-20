using System;
using Datalust.Piggy.Cli.Features;
using Datalust.Piggy.Database;
using Datalust.Piggy.Update;
using Npgsql;
using Serilog;

namespace Datalust.Piggy.Cli.Commands
{
    [Command("up", "Bring a database up to date")]
    class UpdateCommand : Command
    {
        readonly DefineVariablesFeature _defineVariablesFeature;
        readonly UsernamePasswordFeature _usernamePasswordFeature;
        readonly DatabaseFeature _databaseFeature;
        readonly LoggingFeature _loggingFeature;
        readonly ScriptRootFeature _scriptRootFeature;

        bool _createIfMissing = true;

        public UpdateCommand()
        {
            _databaseFeature = Enable<DatabaseFeature>();
            _usernamePasswordFeature = Enable<UsernamePasswordFeature>();
            _scriptRootFeature = Enable<ScriptRootFeature>();
            _defineVariablesFeature = Enable<DefineVariablesFeature>();

            Options.Add("no-create", "If the database does not already exist, do not attempt to create it", _ => _createIfMissing = false);

            _loggingFeature = Enable<LoggingFeature>();
        }

        protected override int Run()
        {
            _loggingFeature.Configure();

            try
            {
                using var connection = DatabaseConnector.Connect(_databaseFeature.Host!, _databaseFeature.Database!, _usernamePasswordFeature.Username!, _usernamePasswordFeature.Password!, _createIfMissing);
                UpdateSession.ApplyChangeScripts(connection, _scriptRootFeature.ScriptRoot!, _defineVariablesFeature.Variables);

                return 0;
            }
            catch (PostgresException ex)
            {
                Log.Fatal("Could not apply change scripts: {Message} ({SqlState})", ex.MessageText, ex.SqlState);
                return -1;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Could not apply change scripts");
                return -1;
            }
        }
    }
}
