using Npgsql.Logging;
using Serilog;

namespace Datalust.Piggy.Database
{
    public class SerilogLoggingProvider : INpgsqlLoggingProvider
    {
        public NpgsqlLogger CreateLogger(string name)
        {
            return new SerilogLogger(Log.ForContext("SourceContext", name ?? typeof(SerilogLoggingProvider).FullName));
        }
    }
}
