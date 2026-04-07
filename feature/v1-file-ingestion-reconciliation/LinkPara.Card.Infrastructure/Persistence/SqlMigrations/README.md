# SQL Migration Policy

This folder contains database-provider-specific SQL migrations that are executed by `IMigrationConfigurator`.

## Structure

- `PostgreSql/`
- `MsSql/`

## Naming

Use versioned file names:

- `V1_0_0__Short_Description.sql`

Keep descriptions short and stable. Do not rename applied migration files.

## When to use SQL migration vs EF migration

Use EF migrations (`Persistence/Migrations/...`) for model/schema changes managed by EF Core.

Use SQL migrations (`Persistence/SqlMigrations/...`) for:

- provider-specific DDL
- indexes EF cannot model cleanly (concurrent/partial/function indexes)
- triggers/functions/views/materialized views
- data backfill/fix scripts that must run in deployment

Avoid duplicating the same DDL in both EF and SQL migrations.

## Authoring rules

- Make scripts idempotent (`IF NOT EXISTS`, safe `ALTER`, guards).
- Keep scripts forward-only.
- Add one logical change per file.
- Write separate scripts per provider when behavior differs.
