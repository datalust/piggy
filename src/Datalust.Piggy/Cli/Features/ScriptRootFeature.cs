using System.Collections.Generic;

namespace Datalust.Piggy.Cli.Features
{
    class ScriptRootFeature : CommandFeature
    {
        public string? ScriptRoot { get; set; }

        public override void Enable(OptionSet options)
        {
            options.Add(
                "s=|script-root=",
                "The root directory to search for scripts",
                v => ScriptRoot = v);
        }

        public override IEnumerable<string> GetUsageErrors()
        {
            if (!Requirement.IsNonEmpty(ScriptRoot)) yield return Requirement.NonEmptyDescription("script root directory");
        }
    }
}
