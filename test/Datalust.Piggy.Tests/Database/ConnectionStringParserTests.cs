using Datalust.Piggy.Database;
using Xunit;

namespace Datalust.Piggy.Tests.Database
{
    public class ConnectionStringParserTests
    {
        [Fact]
        public void ParsesGoodConnectionStringCorrectly1()
        {
            const string connectionString = "Host=localhost";
            var parts = ConnectionStringParser.Parse(connectionString);
            Assert.Equal("localhost", parts["host"]);
        }

        [Fact]
        public void ParsesGoodConnectionStringCorrectly2()
        {
            const string connectionString = "Host=localhost;Username=hunter2;Password=KenSentMe;Database=Skynet";
            var parts = ConnectionStringParser.Parse(connectionString);
            Assert.Equal("localhost", parts["host"]);
            Assert.Equal("hunter2", parts["username"]);
            Assert.Equal("KenSentMe", parts["password"]);
            Assert.Equal("Skynet", parts["database"]);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(";")]
        [InlineData("=;")]
        [InlineData("==;")]
        [InlineData("=;;")]
        [InlineData("=;;=")]
        [InlineData("=;=;=")]
        [InlineData("Host")]
        [InlineData("Host=")]
        [InlineData(";Host=")]
        [InlineData("Host=;")]
        [InlineData("localhost")]
        [InlineData("=localhost")]
        [InlineData(";=localhost")]
        [InlineData("=localhost;")]
        [InlineData("Host=;;")]
        [InlineData("Host=;=;")]
        [InlineData("Host=;Username=;")]
        [InlineData("Host=;=hunter2;")]
        [InlineData("=localhost;Username=;")]
        [InlineData("=localhost;=hunter2;")]
        [InlineData(";Username=;=KenSentMe;DatabaseSkynet")]
        public void DoesNotParseBadConnectionStrings(string? connectionString)
        {
            var parts = ConnectionStringParser.Parse(connectionString);
            Assert.Empty(parts);
        }
    }
}
