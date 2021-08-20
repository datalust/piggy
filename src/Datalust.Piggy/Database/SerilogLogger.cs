using System;
using Npgsql.Logging;
using Serilog;
using Serilog.Events;

namespace Datalust.Piggy.Database
{
    public class SerilogLogger : NpgsqlLogger
    {
        readonly ILogger _logger;

        public SerilogLogger(ILogger logger)
        {
            _logger = logger;
        }

        public override bool IsEnabled(NpgsqlLogLevel level)
        {
            return _logger.IsEnabled(LogEventLevel.Debug);
        }

        public override void Log(NpgsqlLogLevel level, int connectorId, string msg, Exception? exception = null)
        {
            _logger
                .ForContext("ConnectorId", connectorId)
                .ForContext("NpgsqlLogLevel", level)
                .Debug(exception, "{PostgreSqlMessage}", msg);
        }
    }
}
