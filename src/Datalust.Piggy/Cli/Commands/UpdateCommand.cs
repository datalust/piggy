using System;
using Datalust.Piggy.Cli.Features;
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

        string _scriptRoot;
        bool _createIfMissing = true;

        public UpdateCommand()
        {
            _databaseFeature = Enable<DatabaseFeature>();
            _usernamePasswordFeature = Enable<UsernamePasswordFeature>();

            Options.Add(
                "s=|script-root=",
                "The root directory to search for scripts",
                v => _scriptRoot = v);

            _defineVariablesFeature = Enable<DefineVariablesFeature>();

            Options.Add("no-create", "If the database does not already exist, do not attempt to create it", v => _createIfMissing = false);

            _loggingFeature = Enable<LoggingFeature>();
        }

        protected override int Run()
        {
            _loggingFeature.Configure();

            if (!Require(_scriptRoot, "script root directory"))
                return -1;

            try
            {
                UpdateSession.ApplyChangeScripts(
                    _databaseFeature.Host, _databaseFeature.Database, _usernamePasswordFeature.Username, _usernamePasswordFeature.Password,
                    _createIfMissing, _scriptRoot, _defineVariablesFeature.Variables);

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
