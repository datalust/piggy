using System;
using System.Collections.Generic;
using System.Linq;

namespace Datalust.Piggy.Database
{
    public static class ConnectionStringParser
    {
        const StringSplitOptions Options = StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries;

        // e.g. Host=localhost;Username=postgres;Password=password-value;Database=database-name
        public static Dictionary<string, string> Parse(string? connectionString)
        {
            if (connectionString == null) return new Dictionary<string, string>(0);

            var parts = connectionString.Split(';', Options)
                .Where(i => i.Contains('='))
                .Select(i => i.Split('=', 2, Options))
                .Where(a => a.Length == 2)
                .ToDictionary(a => a[0].Trim(), a => a[1].Trim(), StringComparer.InvariantCultureIgnoreCase);

            return parts;
        }
    }
}
