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
            Assert.Equal("localhost", parts["Host"]);
        }

        [Fact]
        public void ParsesGoodConnectionStringCorrectly2()
        {
            const string connectionString = "Host=localhost;Username=hunter2;Password=KenSentMe;Database=Skynet";
            var parts = ConnectionStringParser.Parse(connectionString);
            Assert.Equal("localhost", parts["Host"]);
            Assert.Equal("hunter2", parts["Username"]);
            Assert.Equal("KenSentMe", parts["Password"]);
            Assert.Equal("Skynet", parts["Database"]);
        }

        [Fact]
        public void ParserHandlesDuplicateConnectionStringKeysCorrectly()
        {
            const string connectionString = "Host=localhost;Host=127.0.0.1";
            var parts = ConnectionStringParser.Parse(connectionString);
            Assert.Equal("localhost", parts["Host"]);
        }

        [Fact]
        public void ParserHandlesDuplicateButDifferentCaseConnectionStringKeysCorrectly()
        {
            const string connectionString = "Host=localhost;host=127.0.0.1";
            var parts = ConnectionStringParser.Parse(connectionString);
            Assert.Equal("localhost", parts["Host"]);
            Assert.Equal("127.0.0.1", parts["host"]);
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
