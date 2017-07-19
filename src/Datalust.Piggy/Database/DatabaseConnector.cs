using Npgsql;
using Npgsql.Logging;
using Serilog;

namespace Datalust.Piggy.Database
{
    static class DatabaseConnector
    {
        static DatabaseConnector()
        {
            NpgsqlLogManager.Provider = new SerilogLoggingProvider();
        }

        public static NpgsqlConnection Connect(string host, string database, string username, string password, bool createIfMissing)
        {
            var cstr = $"Host={host};Username={username};Password={password};Database={database}";
            NpgsqlConnection conn = null;
            try
            {
                conn = new NpgsqlConnection(cstr);
                conn.Open();
                return conn;
            }
            catch (PostgresException px) when (px.SqlState == "3D000")
            {
                conn?.Dispose();

                if (createIfMissing && TryCreate(host, database, username, password))
                    return Connect(host, database, username, password, false);

                throw;
            }
        }

        static bool TryCreate(string host, string database, string username, string password)
        {
            var postgresCstr = $"Host={host};Username={username};Password={password};Database=postgres";
            using (var postgresConn = new NpgsqlConnection(postgresCstr))
            {
                postgresConn.Open();

                Log.Information("Creating database {Database} on {Host} with owner {Owner} and default options", database, host, username);
                using (var createCommand = new NpgsqlCommand($"CREATE DATABASE {database} WITH OWNER = {username} ENCODING = 'UTF8' CONNECTION LIMIT = -1;", postgresConn))
                {
                    createCommand.ExecuteNonQuery();
                    Log.Information("Database created successfully");
                    return true;
                }
            }
        }
    }
}