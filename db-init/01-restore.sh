#!/bin/sh
set -e
DB_NAME="${POSTGRES_DB:-import_db}"
DUMP_FILE="/docker-entrypoint-initdb.d/backup.dump"

echo "[init] Checking for automatic restore of $DUMP_FILE into $DB_NAME"
if [ ! -f "$DUMP_FILE" ]; then
  echo "[init] Dump file not found, skipping restore.";
  exit 0;
fi

# Only restore if database is empty (no user tables except migrations or completely fresh)
TABLE_COUNT=$(psql -U "$POSTGRES_USER" -d "$DB_NAME" -Atc "SELECT count(*) FROM information_schema.tables WHERE table_schema='public';") || TABLE_COUNT=0
if [ "$TABLE_COUNT" -gt 0 ]; then
  echo "[init] Database already has $TABLE_COUNT tables; skipping restore.";
  exit 0;
fi

echo "[init] Restoring dump into $DB_NAME ..."
pg_restore -U "$POSTGRES_USER" -d "$DB_NAME" "$DUMP_FILE"
echo "[init] Restore completed."
