using System.Linq;
using Datalust.Piggy.Cli;
using Xunit;

namespace Datalust.Piggy.Tests;

public class PiggyModuleTests
{
    [Fact]
    public void AllCommandsScannedAndFoundAreExplicitlyRegisteredInPiggyModule()
    {
        var expectedCommands = typeof(PiggyModule).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(Command)));
        var actualCommands = PiggyModule.RegisteredCommands;

        foreach (var expected in expectedCommands)
        {
            Assert.Contains(expected, actualCommands);
        }
    }
}
