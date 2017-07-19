using System.Reflection;
using Autofac;
using Datalust.Piggy.Cli;

namespace Datalust.Piggy
{
    class PiggyModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<CommandLineHost>();
            builder.RegisterAssemblyTypes(typeof(Program).GetTypeInfo().Assembly)
                .As<Command>()
                .WithMetadataFrom<CommandAttribute>();
        }
    }
}