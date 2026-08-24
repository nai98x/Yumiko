#!/usr/bin/env bash
set -euo pipefail

# Daily dump of Yumiko's database: pg_dump -> gpg -> rclone, keeping the last $RETENER copies.
# Every value that describes the server (database, role, host, paths, remote) comes from the config
# file: nothing of that is versioned here. See backup.env.example.
# The variable names match AnilistConEnie's backup.env on purpose: both bots run on the same
# server against the same bucket, so a config file of one can be copied over to the other.

CONF="${BACKUP_CONF:-$HOME/.config/yumiko-backup/backup.env}"
[[ -f "$CONF" ]] || { echo "Missing configuration file: $CONF" >&2; exit 1; }
# shellcheck source=/dev/null
source "$CONF"

: "${PGDATABASE:?set PGDATABASE in $CONF}"
: "${PGHOST:?set PGHOST in $CONF}"
: "${PGPORT:?set PGPORT in $CONF}"
: "${PGUSER:?set PGUSER in $CONF}"
: "${REMOTE:?set REMOTE in $CONF}"
: "${PASSFILE:?set PASSFILE in $CONF}"
: "${RCLONE:?set RCLONE in $CONF}"
: "${ESTADO:?set ESTADO in $CONF}"

export PGDATABASE PGHOST PGPORT PGUSER

RETENER="${RETENER:-5}"
PREFIJO="${PREFIJO:-yumiko}"
# Time zone of the date in the file name and in the mark. UTC unless the config says otherwise.
TZ_BACKUP="${TZ_BACKUP:-UTC}"

TMP="$(mktemp -d)"
trap 'rm -rf "$TMP"' EXIT

FECHA="$(TZ="$TZ_BACKUP" date +%Y-%m-%d)"
ARCH="$TMP/$PREFIJO-$FECHA.dump"

pg_dump --format=custom --compress=9 --file="$ARCH"

gpg --batch --yes --symmetric --cipher-algo AES256 --pinentry-mode loopback --passphrase-file "$PASSFILE" --output "$ARCH.gpg" "$ARCH"
ARCH="$ARCH.gpg"

"$RCLONE" copyto "$ARCH" "$REMOTE/$(basename "$ARCH")" --checksum

# Mark the bot reads to warn when a day had no backup. It is written only here: if anything
# failed before, it keeps the old date and the check catches it.
echo "$FECHA" > "$ESTADO"

# Retention: the $RETENER newest files are kept (the date in the name sorts on its own).
# Pruning runs after the upload, so a streak of failures cannot end up emptying the remote.
"$RCLONE" lsf "$REMOTE" --files-only | sort | head -n "-$RETENER" | while read -r f; do
    "$RCLONE" deletefile "$REMOTE/$f"
done
