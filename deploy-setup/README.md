# Deploy

> Everything that belongs to the server —database, roles, host, port, remote, bucket, user— is
> written as a `<placeholder>` throughout this document: none of it is versioned. The real values
> live on the server, in `~/bots/secrets/yumiko.env` and in the backup's `backup.env`.

Everything that runs on the machine: the bot and the database backup. Both are systemd **user
services** under the same user (which requires `loginctl enable-linger`).

- `yumiko.service` — the bot.
- `backup-db.sh`, `yumiko-backup-db.service`, `yumiko-backup-db.timer`, `backup.env.example` — the
  daily backup (see [Database backup](#database-backup)).

CI **only deploys the bot**: any other file of this directory has to be copied to the server by hand
when it changes.

---

# The bot

The bot is deployed through **CI/CD on GitHub Actions** (`.github/workflows/deploy.yml`). The job
runs on a **GitHub-hosted** runner (`ubuntu-latest`): `push` to `master` → restore → vulnerable
package scan → build → test → `dotnet publish -r linux-arm64 --no-self-contained` → packs a
`publish.tar.gz` → copies it to the server over **SCP** → unpacks it into `~/bots/Yumiko-app` and
restarts over **SSH** with `systemctl --user`.

The process is managed by **systemd (user service)**: it outlives the SSH session of the deploy,
restarts on its own and logs to journald.

## Secrets

None of them is versioned. They are all resolved through the **same mechanism**
(`IConfiguration`): environment variables on the server (set by the systemd unit), User Secrets
locally.

| Key | Required | What for                              |
|---|---|---------------------------------------|
| `discordToken` | yes | Discord bot token                     |
| `ConnectionStrings:Database` | yes | Database                              |
| `openWeatherMapToken` | yes | `/weather`                            |
| `theCatApiToken` | yes | `/cat`                                |
| `theDogApiToken` | yes | `/dog`                                |
| `AnilistApiClientId` | yes | OAuth URL of `/anilist setprofile`    |
| `topggToken` | no | Publish stats and read votes on top.gg |
| `Backups:StatePath` | release only | Path of the backup mark (see [Database backup](#database-backup)) |

`Backups:StatePath` is not a secret, but it does not belong in `appsettings.json` either: that file
is public and the path carries the server user name. Scheduled tasks do not run in DEBUG, so it is
not required there.

The six required ones are validated at startup: if any is missing the process fails right away
instead of blowing up when someone uses the command. On top of that, if the database does not answer
once the guilds finish downloading, the bot is marked as not initialized and answers that it is not
ready instead of failing command by command.

Guild and channel ids and `Website` **are not secrets** and live in `appsettings.json`.

## Local setup (once)

```bash
dotnet user-secrets --project src/Yumiko.Bot set discordToken "YOUR_TOKEN"
dotnet user-secrets --project src/Yumiko.Bot set "ConnectionStrings:Database" 'Host=...;Port=...;Database=...;Username=...;Password=...'
dotnet user-secrets --project src/Yumiko.Bot set openWeatherMapToken "..."
dotnet user-secrets --project src/Yumiko.Bot set theCatApiToken "..."
dotnet user-secrets --project src/Yumiko.Bot set theDogApiToken "..."
dotnet user-secrets --project src/Yumiko.Bot set AnilistApiClientId "..."
```

Locally the database is reached through an SSH tunnel against the server, same as in AnilistConEnie.

> If a value contains `$`, use **single** quotes: in fish/bash the double ones expand it and store
> the secret truncated.

On a local **Debug** build the commands are registered only on the `Ids:LogGuildId` guild, so it can
be tested without touching the public instance. Never run a Release build against the production
token before doing the full smoke test.

## Server setup (once)

1. Runtime and layout. The publish is `--no-self-contained`, so the server needs the **.NET 10
   runtime** (`dotnet --list-runtimes` has to show `Microsoft.NETCore.App 10.x`). If AnilistConEnie
   already runs there, it is covered.

   This layout (`~/bots/...`) replaces the old one, a clone of the repo in `~/Yumiko` running under
   `nohup`. Before the first deploy that instance has to be brought down (`pkill Yumiko`) and taken
   out of any autostart, or two bots end up sharing the same token.

2. Create the stable directories:

   ```bash
   mkdir -p ~/bots/secrets ~/bots/Yumiko-app
   ```

3. Create the database. PostgreSQL already runs on the server for AnilistConEnie: Yumiko uses the
   **same instance with its own database and role**, so that neither bot can touch the other's data.
   As `postgres` (`sudo -u postgres psql`):

   ```sql
   CREATE ROLE <bot_role> LOGIN PASSWORD 'GENERATE_ONE';
   CREATE DATABASE <database> OWNER <bot_role>;
   ```

   The bot role must not be a superuser. Owning its own database is enough; to separate it further,
   the database can be created under another owner and `<bot_role>` given only `CONNECT`, `USAGE` on
   the schema and `EXECUTE` on the functions of `db/procedures/` plus the table permissions those
   functions use.

   Apply the schema and the stored procedures (idempotent, they can be reapplied):

   ```bash
   for f in db/schema/*.sql db/procedures/*.sql; do psql -d <database> -f "$f"; done   # or by hand through DBeaver
   ```

   Same as with AnilistConEnie: PostgreSQL has to listen on localhost **only**
   (`listen_addresses = 'localhost'`, verifiable with `ss -tlnp | grep 5432`) and `pg_hba.conf` has
   to require `scram-sha-256` for local connections. With the bot on the same machine TLS is not
   needed; if the database ever moves to another host, add `SSL Mode=Require` to the connection
   string.

   The daily backup of this database is its own (AnilistConEnie's only dumps its own): see
   [Database backup](#database-backup) below.

4. Put the `yumiko.env` file, which is **not versioned**, into `~/bots/secrets/`. Here it is read by
   systemd, not by a shell: the values go literal, without quotes.

   ```
   discordToken=...
   ConnectionStrings__Database=Host=...;Port=...;Database=...;Username=...;Password=...
   openWeatherMapToken=...
   theCatApiToken=...
   theDogApiToken=...
   AnilistApiClientId=...
   topggToken=...
   Backups__StatePath=<absolute path of the backup.env ESTADO>
   ```

   `Backups__StatePath` accepts neither `%h` nor `$HOME`: systemd expands nothing inside an
   `EnvironmentFile`, so it takes the absolute path (`/home/<user>/...`). It has to match the
   `ESTADO` of the backup's `backup.env` exactly.

   Permissions: `chmod 700 ~/bots/secrets && chmod 600 ~/bots/secrets/*`. Only the user running the
   service should be able to read them.

5. Install the service (`yumiko.service` of this directory):

   ```bash
   cp deploy-setup/yumiko.service ~/.config/systemd/user/
   loginctl enable-linger "$USER"        # starts with no session logged in
   systemctl --user daemon-reload
   systemctl --user enable yumiko
   ```

6. On GitHub (Settings → Secrets and variables → Actions), configure the deploy secrets:
   - `HOST` — host or IP of the server.
   - `USERNAME` — SSH user (the same one running the systemd service).
   - `PRIVATE_KEY` — SSH private key with access to that user.

## Operation

```bash
systemctl --user status yumiko
journalctl --user -u yumiko -f
systemctl --user restart yumiko
```

The deploy preserves the `logs/` folder of the app directory: Serilog writes one file per day there
and `/owner logs` returns the most recent one.

If PostgreSQL is not up when the bot finishes downloading the guilds, the bot **does not crash**: it
stays marked as not initialized (logging `Could not connect to the database`) and answers that it is
not ready. It does not retry on its own: bring the database up and run
`systemctl --user restart yumiko`.

## Notes

- The publish targets **`linux-arm64`**. If the server is not ARM64, change the `-r` of the workflow
  (the architecture also affects the SkiaSharp native assets).
- The first time it is worth doing a dry run: point the workflow at `~/bots/Yumiko-app-test` with a
  `yumiko-test.service` before touching the live unit.

---

# Database backup

Daily PostgreSQL dump, encrypted, uploaded to the **same bucket as AnilistConEnie** but into a
**subfolder of its own**, keeping the **last 5 copies**.

The concrete values (database, remote, bucket) **are not versioned**: they live in
`~/.config/yumiko-backup/backup.env` on the server. See `backup.env.example`.

That file has the same format as AnilistConEnie's, so that one can be copied over changing
`PGDATABASE`, `PGUSER`, `REMOTE` and `PREFIJO`. What is actually shared is the rclone remote, the
bucket and the **passphrase**: pointing `PASSFILE` at the same one AnilistConEnie uses leaves a
single secret to keep off the server and a single restore command to learn.

The script carries no default for anything of the infrastructure (database, role, host, port, paths,
remote): it all comes from the `backup.env`, and a missing key fails at startup naming it. The only
things it assumes are `RETENER=5`, `PREFIJO=yumiko` and `TZ_BACKUP=UTC`.

The variable names are in Spanish because they match AnilistConEnie's `backup.env`: both bots run on
the same server against the same bucket, and keeping one format means a config file of either can be
copied to the other.

Retention looks **inside the subfolder only**, so neither bot prunes the other's backups.

## Pieces

- `backup-db.sh` → `~/bin/yumiko-backup-db.sh` (`chmod +x`)
- `yumiko-backup-db.service` / `yumiko-backup-db.timer` → `~/.config/systemd/user/`
- `backup.env.example` → copy to `~/.config/yumiko-backup/backup.env` (`chmod 600`) and fill in

If the script on the server goes stale the backup may keep uploading and fail later, unnoticed
except for the bot's warning.

The unit names carry the `yumiko-` prefix because they live alongside AnilistConEnie's under the
same systemd user.

## Requirements on the server

1. **rclone** in `~/bin/rclone` with the remote already configured for AnilistConEnie: it is reused
   as is. If the provider key is restricted to one bucket, `rclone lsd remote:` fails by design:
   check with `rclone lsf remote:bucket`.

2. **Read-only role** for the dump (the bot role will not do: it owns the database, and a credential
   with write access does not belong in a cron job):

   ```sql
   CREATE ROLE <backup_role> LOGIN PASSWORD '...';
   GRANT CONNECT ON DATABASE <database> TO <backup_role>;
   GRANT USAGE ON SCHEMA public TO <backup_role>;
   GRANT pg_read_all_data TO <backup_role>;  -- PostgreSQL 14+
   ```

   And add a `<host>:<port>:<database>:<backup_role>:<password>` line to `~/.pgpass` (`chmod 600`).
   That file accumulates one line per connection: AnilistConEnie's is still there, and they do not
   collide because database and role differ. If the file is not `chmod 600`, libpq ignores it
   silently and the dump fails asking for a password nobody can type from the timer.

3. **Passphrase**: reuse AnilistConEnie's by pointing `PASSFILE` at its file. To use a separate one,
   generate it with `head -c 32 /dev/urandom | base64`, leave it in a `chmod 600` file and point
   `PASSFILE` there. Keep it **off the server** (password manager): it is the only piece of the
   setup that cannot be recovered if the machine is lost, and without it the backups are worthless.

4. The bucket lifecycle (*keep only the last version*, if the provider versions objects) is already
   configured for AnilistConEnie and applies to the whole bucket.

## Installation

```bash
cp deploy-setup/backup-db.sh ~/bin/yumiko-backup-db.sh
chmod +x ~/bin/yumiko-backup-db.sh
cp deploy-setup/yumiko-backup-db.{service,timer} ~/.config/systemd/user/
mkdir -p ~/.config/yumiko-backup && cp deploy-setup/backup.env.example ~/.config/yumiko-backup/backup.env
chmod 600 ~/.config/yumiko-backup/backup.env    # fill in the real values

systemctl --user daemon-reload
systemctl --user enable --now yumiko-backup-db.timer
systemctl --user list-timers yumiko-backup-db.timer
```

First run by hand, to validate the whole setup (dump, encryption, upload and pruning):

```bash
systemctl --user start yumiko-backup-db.service
journalctl --user -u yumiko-backup-db -n 50 --no-pager
~/bin/rclone lsf <the REMOTE of the backup.env>
```

`OnCalendar` is interpreted in the **local time of the server**, not in UTC.

## Restore

```bash
rclone copy <remote>:<bucket>/<subfolder>/yumiko-YYYY-MM-DD.dump.gpg /tmp/
gpg --batch --pinentry-mode loopback --passphrase-file <the PASSFILE of the backup.env> \
    -o /tmp/y.dump -d /tmp/yumiko-YYYY-MM-DD.dump.gpg
pg_restore -d <target_database> --no-owner --clean --if-exists /tmp/y.dump
```

`--no-owner` because the dump is produced by the read-only role and the restore runs under another
one.

The dump covers a single database: it **does not include the roles nor their permissions**, which
are recreated with the SQL above and what is in `db/README.md`. It does not cover
`~/bots/secrets/yumiko.env` either, which is not versioned.

## Failure warning

After uploading, the script writes the date into the `ESTADO` file. The bot
(`BackupScheduledService`) reads it every day at **13:00 UTC** and, when the mark is not today's,
warns in the **configBots** channel (`Ids:Channels:ConfigBots` of `appsettings.json`) of the logs
guild. The path reaches it through the `Backups__StatePath` environment variable and has to match
the one in the `backup.env`.

It verifies that the script finished well, not that the file is still at the provider: it covers the
real failures (timer down, expired credentials, broken dump) but not someone emptying the bucket.

The "is it up to date" rule lives in `BackupState` (Application) and is covered by tests; the bot
only reads the file and sends the embed.

## Verification

It is worth re-proving the restore every now and then against a throwaway database, comparing
against the live one:

```bash
psql -d <database> -c "select count(*) from anilist_users"
psql -d <test_database> -c "select count(*) from anilist_users"
psql -d <test_database> -c "select count(*) from pg_proc where pronamespace = 'public'::regnamespace"
```

The last one has to match the number of files in `db/procedures/`.

The user journal may not be persisted; in that case the logs are read from the system journal:

```bash
sudo journalctl _SYSTEMD_USER_UNIT=yumiko-backup-db.service -n 50 --no-pager
```
