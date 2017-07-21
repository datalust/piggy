![Piggy](https://raw.githubusercontent.com/datalust/piggy/master/asset/Piggy-400px.png)

A friendly PostgreSQL script runner in the spirit of [DbUp](https://github.com/DbUp/DbUp).

[![Build status](https://ci.appveyor.com/api/projects/status/889gkdpvjbjuhkfg?svg=true)](https://ci.appveyor.com/project/datalust/piggy)

### What is Piggy?

Piggy is a simple command-line tool for managing schema and data changes to PostgreSQL databases. Piggy looks for `.sql` files in a directory and applies them to the database in order, using transactions and a change log table to ensure each script runs only once per database.

### Installation

Piggy is available as a Windows MSI installer from [the releases page](https://github.com/datalust/piggy/releases). Linux and macOS are supported, but currently require the `Datalust.Piggy` project in this repository to be built from source.

### Organizing change scripts

Your `.sql` files should be named so that they will sort lexicographically in the order they need to be executed:

```
001-create-schema.sql
002-create-users-table.sql
003-...
```

If the scripts are arranged in subfolders, these can be ordered as well:

```
v1/
  001-create-schema.sql
  002-create-users-table.sql
  003-...
v2/
  001-rename-users-table.sql
```

Each script is just regular SQL with any DDL or DML statements required to make a change to the database.

### Applying scripts

Scripts must be applied starting from a _script root_. Piggy searches for `.sql` files and generates script names like `/v1/001-create-schema.sql` using each script's filename relative to the script root. These relative filenames are checked against the change log table to determine which of them need to be run.

The script root is specified with `-s`. Other parameters identify the database server host, the database name, username and password:

```
piggy apply -s <scripts> -h <host> -d <database> -u <username> -p <password>
```

If the database does not exist, Piggy will create it using sensible defaults. To opt out of this behavior, add the `--no-create` flag.

For more detailed usage information, run `piggy help apply`; to see all available commands run `piggy help`.

### Transactions

Piggy wraps each script in a transaction that also covers the change log table update. A few DDL statements can't run within a transaction in PostgreSQL - in these cases, add:

```sql
-- PIGGY NO TRANSACTION
```

as the first line of the script.

### Variable substitution

Piggy uses `$var$` syntax for replaced variables:

```sql
create table $schema$.users;
insert into users (name) values ('$admin$');
```

Values are inserted using pure text substitution: no escaping or other processing is applied. If no value is supplied for a variable that appears in a script, Piggy will leave the script unchanged (undefined variables will not be replaced with the empty string).

Variables are supplied on the command-line with the `-v` flag:

```
piggy apply ... -v schema=myapp -v admin=myuser
```

### Change log

The change log is stored in the target database in `piggy.changes`. The `piggy log` command can be used to view applied change scripts.
