using Autofac;
using Datalust.Piggy.Cli;
using Serilog;

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
                var exit = clh.Run(args);
                Log.CloseAndFlush();
                return exit;
            }
        }
    }
}