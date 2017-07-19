using Autofac;
using Datalust.Piggy.Cli;

namespace Datalust.Piggy
{
    class Program
    {
        static int Main(string[] args)
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<PiggyModule>();

            using (var container = builder.Build())
            {
                var clh = container.Resolve<CommandLineHost>();
                return clh.Run(args);
            }
        }
    }
}