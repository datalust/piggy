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
            var parts = new Dictionary<string, string>();

            if (connectionString == null) return parts;

            connectionString.Split(';', Options)
                .Where(s => s.Contains('='))
                .Select(s => s.Split('=', 2, Options))
                .Where(a => a.Length == 2)
                .ToList()
                .ForEach(a => parts.TryAdd(a[0].Trim(), a[1].Trim()));

            return parts;
        }
    }
}
