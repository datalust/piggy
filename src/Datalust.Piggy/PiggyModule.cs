using System;
using Autofac;
using Datalust.Piggy.Cli;
using Datalust.Piggy.Cli.Commands;

namespace Datalust.Piggy
{
    public class PiggyModule : Module
    {
        public static readonly Type[] RegisteredCommands =
        {
            typeof(BaselineCommand),
            typeof(HelpCommand),
            typeof(LogCommand),
            typeof(PendingCommand),
            typeof(UpdateCommand),
            typeof(VersionCommand)
        };

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<CommandLineHost>();
            builder.RegisterTypes(RegisteredCommands).As<Command>().WithMetadataFrom<CommandAttribute>();
        }
    }
}
