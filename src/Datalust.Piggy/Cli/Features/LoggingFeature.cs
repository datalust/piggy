using System;
using Serilog;
using Serilog.Events;

namespace Datalust.Piggy.Cli.Features
{
    class LoggingFeature : CommandFeature
    {
        string _serverUrl, _apiKey;
        LogEventLevel _level = LogEventLevel.Information;

        public override void Enable(OptionSet options)
        {
            options.Add("log-seq=", "Log output to a Seq server at the specified URL", v => _serverUrl = v);
            options.Add("log-seq-apikey=", "If logging to Seq, an optional API key", v => _apiKey = v);
            options.Add("log-debug", "Write additional diagnostic log output", v => _level = LogEventLevel.Debug);
        }

        public void Configure()
        {
            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Is(_level)
                .Enrich.WithProperty("Invocation", Guid.NewGuid())
                .WriteTo.Console();

            if (!string.IsNullOrWhiteSpace(_serverUrl))
                loggerConfiguration
                    .Enrich.WithProperty("Application", "Piggy")
                    .WriteTo.Seq(_serverUrl, apiKey: _apiKey);

            Log.Logger = loggerConfiguration.CreateLogger();
        }
    }
}
