using System;
using System.Reflection;

namespace Datalust.Piggy.Cli.Commands
{
    [Command("--version", "Print the current executable version")]
    class VersionCommand : Command
    {
        protected override int Run()
        {
<<<<<<< HEAD
            var version = GetVersion();
=======
            var version = Assembly.GetEntryAssembly()!.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;
>>>>>>> 71bfa0c (Nullable reference types)
            Console.WriteLine(version);
            return 0;
        }

        public static string GetVersion()
        {
            return typeof(VersionCommand).GetTypeInfo().Assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        }
    }
}
