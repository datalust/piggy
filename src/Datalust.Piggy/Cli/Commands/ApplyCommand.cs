using System;
using Datalust.Piggy.Apply;
using Datalust.Piggy.Cli.Features;
using Serilog;

namespace Datalust.Piggy.Cli.Commands
{
    [Command("apply", "Apply a set of database change scripts")]
    class ApplyCommand : Command
    {
        readonly DefineVariablesFeature _defineVariablesFeature;
        readonly UsernamePasswordFeature _usernamePasswordFeature;
        readonly DatabaseFeature _databaseFeature;
        readonly LoggingFeature _loggingFeature;

        string _scriptRoot;
        bool _createIfMissing = true;

        public ApplyCommand()
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

            if (!(Require(_databaseFeature.Host, "host") && Require(_databaseFeature.Database, "database") &&
                Require("username", _usernamePasswordFeature.Username) && Require("password", _usernamePasswordFeature.Password) &&
                Require(_scriptRoot, "script root directory")))
                return -1;

            try
            {
                ApplySession.ApplyChangeScripts(
                    _databaseFeature.Host, _databaseFeature.Database, _usernamePasswordFeature.Username, _usernamePasswordFeature.Password,
                    _createIfMissing, _scriptRoot, _defineVariablesFeature.Variables);

                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Could not apply change scripts");
                return -1;
            }
        }
    }
}
