using System;
using Datalust.Piggy.Baseline;
using Datalust.Piggy.Cli.Features;
using Datalust.Piggy.Update;
using Npgsql;
using Serilog;

namespace Datalust.Piggy.Cli.Commands
{
    [Command("baseline", "Add scripts to the change log without running them")]
    class BaselineCommand : Command
    {
        readonly UsernamePasswordFeature _usernamePasswordFeature;
        readonly DatabaseFeature _databaseFeature;
        readonly LoggingFeature _loggingFeature;
        readonly ScriptRootFeature _scriptRootFeature;

        public BaselineCommand()
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
                BaselineSession.BaselineDatabase(
                    _databaseFeature.Host, _databaseFeature.Database, _usernamePasswordFeature.Username, _usernamePasswordFeature.Password,
                    _scriptRootFeature.ScriptRoot);

                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Could not baseline the database");
                return -1;
            }
        }
    }
}
