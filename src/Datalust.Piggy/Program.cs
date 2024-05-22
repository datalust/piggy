using Autofac;
using Datalust.Piggy;
using Datalust.Piggy.Cli;
using Serilog;

var builder = new ContainerBuilder();
builder.RegisterModule<PiggyModule>();

await using var container = builder.Build();
var commandLineHost = container.Resolve<CommandLineHost>();
var exit = commandLineHost.Run(args);
await Log.CloseAndFlushAsync();
return exit;
