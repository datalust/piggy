using System.Collections.Generic;

namespace Datalust.Piggy.Cli.Features
{
    class UsernamePasswordFeature : CommandFeature
    {
        public string? Username { get; private set; }

        public string? Password { get; private set; }

        public override void Enable(OptionSet options)
        {
            options.Add("u=|username=", "The name of the user account", v => Username = v);
            options.Add("p=|password=", "The password for the user account", v => Password = v);
        }

        public override IEnumerable<string> GetUsageErrors()
        {
            if (!Requirement.IsNonEmpty(Username)) yield return Requirement.NonEmptyDescription("username");
            if (!Requirement.IsNonEmpty(Password)) yield return Requirement.NonEmptyDescription("password");
        }
    }
}
