# Database

Versioned schema and stored procedures. The database is **database-first**: the schema is designed
here (it is the source of truth) and the C# code adapts to it. The bot reaches it through **Dapper
invoking stored procedures** — never through SQL embedded in the code.

Same criterion as AnilistConEnie, with one difference: here **tables, columns and functions are in
English**, like the rest of the code of this repo.

Values that belong to the server (database name, roles, passwords) are written as `<placeholders>`:
they are not versioned.

## Layout

```
db/
  schema/        # tables, indexes, constraints
  procedures/    # one .sql per stored procedure
```

## Tables

| Table | What it holds |
|---|---|
| `anilist_users` | Link between a Discord account and an AniList one. Global, not per guild |
| `higher_or_lower_scores` | Higher or Lower record per user and guild |
| `higher_or_lower_duo_scores` | Higher or Lower duo record per pair of users and guild. The pair is stored ordered (`first_user_id < second_user_id`) |
| `quiz_stats` | Accumulated trivia stats per user, guild, gamemode and difficulty |

Two quirks of `quiz_stats`:

- `accuracy_percentage` is stored, computed with **integer division** over the accumulated totals.
  Turning it into a decimal re-ranks every leaderboard that already exists.
- In `Genres` mode, the `difficulty` column holds the **genre name** instead of a difficulty.

`gamemode` and `difficulty` hold the names of the C# enums (`Characters`, `Easy`, …), never the
Spanish labels shown in the embeds.

## Script conventions

- **Idempotent**: every script can be run more than once without breaking (`CREATE TABLE IF NOT
  EXISTS`, `CREATE OR REPLACE FUNCTION`).
- **One stored procedure per file** under `procedures/`, named after the stored procedure.
- Schema changes and the stored procedures that consume them go in the **same commit** as the C#
  code that uses them.
- **Migrations are not versioned**: `schema/` always reflects the current state of each table (the
  clean `CREATE`), with no `ALTER` and no data scripts.

## Applying the scripts

Connected to the database (through DBeaver or `psql`), run the ones in `schema/` first and the ones
in `procedures/` afterwards. Being idempotent, reapplying them syncs the database with what is
versioned.

```bash
for f in db/schema/*.sql db/procedures/*.sql; do psql -d <database> -f "$f"; done
```

## Security of the bot role

The role the bot connects with must not be a superuser nor own the schema: `CONNECT` to the
database, `USAGE` on the schema and `EXECUTE` on the functions of `procedures/` are enough (plus
whatever table permissions those functions need). The database listens on localhost only.

## AnilistConEnie

AnilistConEnie writes the AniList link into `anilist_users`. It connects with a role of its own that
only has `EXECUTE` on `anilist_user_upsert`:

```sql
CREATE ROLE <anilistconenie_role> LOGIN PASSWORD '...';
GRANT CONNECT ON DATABASE <database> TO <anilistconenie_role>;
GRANT USAGE ON SCHEMA public TO <anilistconenie_role>;
GRANT EXECUTE ON FUNCTION anilist_user_upsert(bigint, integer) TO <anilistconenie_role>;
GRANT INSERT, UPDATE ON anilist_users TO <anilistconenie_role>;
```
