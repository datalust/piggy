using System;
using System.Reflection;

namespace Datalust.Piggy.Cli.Commands
{
    [Command("--version", "Print the current executable version")]
    class VersionCommand : Command
    {
        protected override int Run()
        {
            var version = typeof(VersionCommand).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
            Console.WriteLine(version);
            return 0;
        }
    }
}
