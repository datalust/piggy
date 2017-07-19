using Dapper;
using Datalust.Piggy.Apply;
using Npgsql;
using Serilog;

namespace Datalust.Piggy.History
{
    static class AppliedChangeScriptLog
    {
        const string ChangesTableSchema = "piggy";
        const string ChangesTableName = ChangesTableSchema + ".changes";

        public const string ChangesTableCreateScript =
@"CREATE SCHEMA IF NOT EXISTS " + ChangesTableSchema + @";

CREATE TABLE IF NOT EXISTS " + ChangesTableName + @" (
    scriptfile varchar(1024) primary key,
    appliedat timestamptz not null default now(),
    appliedby varchar(1024) not null default current_user,
    appliedorder serial not null
);

";

        public static AppliedChangeScript[] GetAppliedChangeScripts(NpgsqlConnection connection)
        {
            try
            {
                return connection.Query<AppliedChangeScript>($"SELECT scriptfile, appliedat, appliedby, appliedorder FROM {ChangesTableName} ORDER BY appliedorder ASC;").AsList().ToArray();
            }
            catch (PostgresException px) when (px.SqlState == "42P01")
            {
                Log.Debug(px, "Change log table does not exist");
                return new AppliedChangeScript[0];
            }
        }

        public static string CreateApplyLogScriptFor(ChangeScriptFile script)
        {
            return "INSERT INTO " + ChangesTableName + $"(scriptfile) VALUES ('{script.RelativeName}');";
        }
    }
}
