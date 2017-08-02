using System.Collections.Generic;

namespace Datalust.Piggy.Cli.Features
{
    class DatabaseFeature : CommandFeature
    {
        public string Host { get; set; }
        public string Database { get; set; }

        public override void Enable(OptionSet options)
        {
            options.Add(
                "h=|host=",
                "The PostgreSQL host",
                v => Host = v);

            options.Add(
                "d=|database=",
                "The database to apply changes to",
                v => Database = v);
        }

        public override IEnumerable<string> GetUsageErrors()
        {
            if (!Requirement.IsNonEmpty(Host)) yield return Requirement.NonEmptyDescription("host");
            if (!Requirement.IsNonEmpty(Database)) yield return Requirement.NonEmptyDescription("database");
        }
    }
}
