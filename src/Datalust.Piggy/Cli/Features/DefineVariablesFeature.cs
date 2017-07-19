using System.Collections.Generic;

namespace Datalust.Piggy.Cli.Features
{
    class DefineVariablesFeature : CommandFeature
    {
        readonly Dictionary<string, string> _variables = new Dictionary<string, string>();

        public Dictionary<string, string> Variables => _variables; 
         
        public override void Enable(OptionSet options)
        {
            options.Add(
                "v={=}|variable={=}",
                "Define variables to be $substituted$ into scripts, e.g. -v Customer=C123 -v Environment=Production",
                (n, v) =>
                {
                    var name = n.Trim();
                    var valueText = v?.Trim() ?? "";
                    _variables.Add(name, valueText);
                });
        }
    }
}
