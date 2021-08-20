using System;
using Serilog;
using Serilog.Events;

namespace Datalust.Piggy.Cli.Features
{
    class LoggingFeature : CommandFeature
    {
<<<<<<< HEAD
        string _serverUrl, _apiKey;
        LogEventLevel _level = LogEventLevel.Information, _consoleLevel = LevelAlias.Minimum;
=======
        string? _serverUrl, _apiKey;
        LogEventLevel _level = LogEventLevel.Information;
>>>>>>> 71bfa0c (Nullable reference types)

        public override void Enable(OptionSet options)
        {
            options.Add("quiet", "Limit diagnostic terminal output to errors only", v => _consoleLevel = LogEventLevel.Error);
            options.Add("debug", "Write additional diagnostic log output", v => _level = LogEventLevel.Debug);
            options.Add("log-seq=", "Log output to a Seq server at the specified URL", v => _serverUrl = v);
            options.Add("log-seq-apikey=", "If logging to Seq, an optional API key", v => _apiKey = v);
        }

        public void Configure()
        {
            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Is(_level)
                .Enrich.WithProperty("Application", "Piggy")
                .Enrich.WithProperty("Invocation", Guid.NewGuid())
                .WriteTo.Console(standardErrorFromLevel: LogEventLevel.Error, restrictedToMinimumLevel: _consoleLevel);

            if (!string.IsNullOrWhiteSpace(_serverUrl))
                loggerConfiguration
                    .WriteTo.Seq(_serverUrl, apiKey: _apiKey);

            Log.Logger = loggerConfiguration.CreateLogger();
        }
    }
}
