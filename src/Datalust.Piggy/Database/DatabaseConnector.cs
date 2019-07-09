using System;
using Npgsql;
using Npgsql.Logging;
using Serilog;

namespace Datalust.Piggy.Database
{
    /// <summary>
    /// Assists with making a PostgreSQL database connection.
    /// </summary>
    public static class DatabaseConnector
    {
        static DatabaseConnector()
        {
            NpgsqlLogManager.Provider = new SerilogLoggingProvider();
        }

        /// <summary>
        /// Connect to the specified database.
        /// </summary>
        /// <param name="host">The PostgreSQL host to connect to.</param>
        /// <param name="database">The database to update.</param>
        /// <param name="username">The username to authenticate with.</param>
        /// <param name="password">The password to authenticate with.</param>
        /// <param name="createIfMissing">If <c>true</c>, Piggy will attempt to create the database if it doesn't exist. The
        /// database must already exist, otherwise.</param>
        /// <returns>An open database connection.</returns>
        public static NpgsqlConnection Connect(string host, string database, string username, string password, bool createIfMissing)
        {
            try
            {
                return Connect($"Host={host};Username={username};Password={password};Database={database}");
            }
            catch (PostgresException px) when (px.SqlState == "3D000")
            {
                if (createIfMissing && TryCreate(host, database, username, password))
                    return Connect(host, database, username, password, false);

                throw;
            }
        }

        /// <summary>
        /// Connect to the specified database.
        /// </summary>
        /// <param name="connectionString">A connection string identifying the database.</param>
        /// <returns>An open database connection.</returns>
        public static NpgsqlConnection Connect(string connectionString)
        {
            if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));

            var conn = new NpgsqlConnection(connectionString);
            
            Log.Information("Connecting to database {Database} on {Host}", conn.Database, conn.Host);

            try
            {
                conn.Open();
            }
            catch
            {
                conn.Dispose();
                throw;
            }

            Log.Information("Connected");

            return conn;
        }

        static bool TryCreate(string host, string database, string username, string password)
        {
            Log.Information("Database does not exist; attempting to create it");

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