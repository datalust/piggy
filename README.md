![Piggy](https://raw.githubusercontent.com/datalust/piggy/master/asset/Piggy-400px.png)

A friendly PostgreSQL script runner in the spirit of [DbUp](https://github.com/DbUp/DbUp).
[sln](sln)
[![Build status](https://ci.appveyor.com/api/projects/status/889gkdpvjbjuhkfg?svg=true)](https://ci.appveyor.com/project/datalust/piggy)

### What is Piggy?

Piggy is a simple command-line tool for managing schema and data changes to PostgreSQL databases. Piggy looks for `.sql` files in a directory and applies them to the database in order, using transactions and a change log table to ensure each script runs only once per database.

### Installation

Piggy is available for Windows, macOS and Linux from [the releases page](https://github.com/datalust/piggy/releases). If your platform of choice isn't listed, please [raise an issue here](https://github.com/datalust/piggy/issues) so that we can add it.

### Workflow

To manage database updates with Piggy, write SQL scripts to make changes, rather than applying changes directly.

Organize change scripts on the file system under a _script root_ directory. Name files so that they sort lexicographically in the order in which they need to be executed:

```
001-create-schema.sql
002-create-users-table.sql
003-...
```

If the scripts are arranged in subdirectories, these must be ordered by name as well:

```
v1/
  001-create-schema.sql
  002-create-users-table.sql
  003-...
v2/
  001-rename-users-table.sql
```

Piggy enumerates `.sql` files and generates names like `/v1/001-create-schema.sql` using each script's filename relative to the script root. These relative names are checked against the change log table to determine which of them need to be run.

To bring a database up to date, run `piggy up`, providing the script root, host, database name, username and password:

```
piggy up -s <scripts> -h <host> -d <database> -u <username> -p <password>
```

If the database does not exist, Piggy will create it using sensible defaults. To opt out of this behavior, add the `--no-create` flag.

Over time, as your application grows, create new scripts to move the database forwards - don't edit the existing ones, since they've already been applied and will be ignored by Piggy.

Piggy can be used to update from any previous schema version to the current one: scripts that have already been run on a database are ignored, so only necessary scripts are applied.

For more detailed usage information, run `piggy help up`; to see all available commands run `piggy help`.

### Transactions

Piggy wraps each script in a transaction that also covers the change log table update. A few DDL statements can't run within a transaction in PostgreSQL - in these cases, add:

```sql
-- PIGGY NO TRANSACTION
```

as the first line of the script.

### Variable substitution

Piggy uses `$var$` syntax for replaced variables:

```sql
create table $schema$.users (name varchar(140) not null);
insert into $schema$.users (name) values ('$admin$');
```

Values are inserted using pure text substitution: no escaping or other processing is applied. If no value is supplied for a variable that appears in a script, Piggy will leave the script unchanged (undefined variables will not be replaced with the empty string).

Variables are supplied on the command-line with the `-v` flag:

```
piggy up ... -v schema=myapp -v admin=myuser
```

### Change log

The change log is stored in the target database in `piggy.changes`. The `piggy log` command is used to view applied change scripts.

### Help

Run `piggy help` to see all available commands, and `piggy help <command>` for detailed command help.

```
> piggy help
Usage: piggy <command> [<args>]

Available commands are:
  baseline   Add scripts to the change log without running them
  help       Show information about available commands
  log        List change scripts that have been applied to a database
  pending    Determine which scripts will be run in an update
  up         Bring a database up to date
  --version  Print the current executable version

Type `piggy help <command>` for detailed help
```

### C&sharp; API

For development and test automation purposes, the core script runner is also packaged as a C&sharp; API and published to NuGet as _Datalust.Piggy_.

```csharp
// dotnet add package Datalust.Piggy
var connectionString = // Npgsql connection string
using (var connection = DatabaseConnector.Connect(connectionString))
{
    UpdateSession.ApplyChangeScripts(connection, "./db", new Dictionary<string, string>());
}
```
