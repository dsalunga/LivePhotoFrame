# LivePhotoFrame Deployment & Migration Runbook

## Overview

This document covers deployment procedures, database migration steps, and rollback instructions for the LivePhotoFrame modernization.

## Prerequisites

- .NET 10 SDK installed (see `global.json` for pinned version band)
- PostgreSQL 15+ (local or remote)
- Node.js 20+ (for frontend build)

## Environment Setup

1. Copy `.env.example` to `.env` and fill in local values.
2. Ensure PostgreSQL is running and accessible.
3. Run `dotnet restore` at solution root.

## Database Migration

### Apply Migrations (PostgreSQL - Default)

```bash
cd LivePhotoFrame.WebApp
dotnet ef database update --context ApplicationDbContext
```

### Apply Migrations (SQL Server - Fallback)

```bash
cd LivePhotoFrame.WebApp
DATABASE_PROVIDER=sqlserver dotnet ef database update --context ApplicationDbContext
```

### Create a New Migration

```bash
cd LivePhotoFrame.WebApp
dotnet ef migrations add <MigrationName> --output-dir Data/Migrations/Postgres
```

## Deployment Steps

### Web Application

1. Build: `dotnet publish LivePhotoFrame.WebApp -c Release -o ./publish`
2. Set environment variables (see `.env.example`)
3. Apply pending migrations: `dotnet ef database update`
4. Start application

### Frontend (Vite)

1. `cd LivePhotoFrame.Frontend && npm ci && npm run build`
2. Output goes to `LivePhotoFrame.WebApp/wwwroot/app/` (served by backend)

## Rollback Procedures

### Database Rollback

```bash
# Roll back to a specific migration
dotnet ef database update <PreviousMigrationName> --context ApplicationDbContext
```

### Application Rollback

1. Stop the current deployment.
2. Deploy the previous known-good build artifact.
3. Roll back database migrations if schema changed.
4. Verify auth flows (login/register/password reset) work.

### Rollback Triggers

- Any migration failure during apply
- Auth flow failures post-deploy
- Unrecoverable data corruption detected
- Provider mismatch errors at startup

## Health Checks

- `GET /health` — basic app health
- Verify database connectivity on startup (logged)
- Verify Identity tables exist

## Cutover: SQL Server to PostgreSQL

1. Ensure PostgreSQL migration set applied on staging.
2. Validate Identity auth flow end-to-end on staging.
3. Back up SQL Server production database.
4. Freeze writes (maintenance mode).
5. Run data migration script (if applicable).
6. Switch `DATABASE_PROVIDER` to `postgres` and deploy.
7. Smoke test auth flows.
8. If failure: revert `DATABASE_PROVIDER` to `sqlserver`, restore backup, redeploy.
