# LivePhotoFrame Modernization Plan Checklist

Status: Implementation in progress  
Last updated: 2026-04-13

## Locked Decisions

- [x] Target web runtime upgrade path: `.NET 10`.
- [x] Default relational database target: `PostgreSQL`.
- [x] Frontend strategy: replace legacy React 15 + Webpack 2 + SpaServices with modern React + Vite.
- [x] Database rollout model: keep support for both SQL Server and PostgreSQL during transition, with PostgreSQL as default.
- [x] Vite deployment model: backend-served static build by default (split deployment can be added later if needed).
- [x] Native roadmap: modernize now to `.NET MAUI` (do not defer).
- [x] macOS target path: `Mac Catalyst` first, with explicit quality gate and fallback decision point.

## Goals

- [x] Upgrade web projects to `.NET 10` cleanly and remove deprecated hosting/tooling patterns.
- [x] Make PostgreSQL the default database for local/dev/prod.
- [x] Preserve existing Identity behavior and login flows during database provider migration.
- [x] Replace legacy SPA stack with a maintainable modern frontend pipeline.
- [x] Deliver a modern native cross-platform app path (Windows, macOS, iOS, Android) via MAUI.
- [x] Add secure network image source support (`HTTPS` default, `HTTP` opt-in only).
- [x] Reduce dependency/vulnerability risk and establish safer upgrade cadence.

## Non-Goals (for this phase)

- [x] No full feature expansion during migration; focus on parity and stability.
- [x] No immediate removal of SQL Server support until PostgreSQL cutover is validated.

---

## Phase 0: Baseline and Safety Rails

- [x] Create `global.json` pinned to a stable `.NET 10` SDK band.
- [x] Capture baseline build behavior and warnings for:
- [x] `LivePhotoFrame.WebApp`
- [x] `LivePhotoFrame.ReactJs`
- [x] Add a migration/rollback runbook doc (`docs/deployment-migration-runbook.md`).
- [x] Add `.env.example` / config template for local DB settings.
- [x] Confirm deployment targets and hosting assumptions for web apps.

## Phase 1: Web Runtime Upgrade to .NET 10

- [x] Update target frameworks:
- [x] `LivePhotoFrame.WebApp/LivePhotoFrame.WebApp.csproj` -> `net10.0`
- [x] `LivePhotoFrame.ReactJs/LivePhotoFrame.ReactJs.csproj` -> `net10.0`
- [x] Upgrade top-level package references in `LivePhotoFrame.WebApp.csproj` to 10.x line:
- [x] `Microsoft.AspNetCore.Identity.EntityFrameworkCore`
- [x] `Microsoft.EntityFrameworkCore`
- [x] `Microsoft.EntityFrameworkCore.Tools`
- [x] Remove obsolete `DotNetCliToolReference` entries from csproj files.
- [x] Migrate to modern hosting model in `LivePhotoFrame.WebApp`:
- [x] Replace `WebHost.CreateDefaultBuilder` with `WebApplication.CreateBuilder`.
- [x] Replace `Startup` pattern with top-level `Program.cs` composition.
- [x] Replace `services.AddMvc()` with `AddControllersWithViews()`.
- [x] Replace `IHostingEnvironment` with `IWebHostEnvironment`.
- [x] Keep endpoint routing behavior equivalent to current behavior.
- [x] Add explicit environment-specific exception handling (`UseExceptionHandler`, HSTS where appropriate).
- [x] Ensure `dotnet build` and `dotnet run` remain clean for `WebApp`.

## Phase 2: PostgreSQL as Default Database

- [x] Add provider package to `LivePhotoFrame.WebApp`:
- [x] `Npgsql.EntityFrameworkCore.PostgreSQL` (EF Core 10-compatible version).
- [x] Keep SQL Server provider support during transition window.
- [x] Replace current hardcoded SQL Server registration in DI:
- [x] from `UseSqlServer(...)`
- [x] to provider-based selection with PostgreSQL as default and SQL Server fallback.
- [x] Add explicit provider option in configuration (example: `DatabaseProvider=postgres` default).
- [x] Update `appsettings.json` with PostgreSQL-first default connection string and optional SQL Server connection string.
- [x] Create PostgreSQL migration set for Identity schema:
- [x] New migration output path, e.g. `Data/Migrations/Postgres`.
- [x] Avoid reusing SQL Server-specific migration artifacts as canonical for PostgreSQL.
- [ ] Validate fresh database creation and migration apply against PostgreSQL.
- [ ] Validate login/register/password reset token flow against PostgreSQL-backed Identity.
- [x] Add startup-time validation/logging for connection/provider mismatch.

## Phase 3: Data Migration and Cutover Plan (if SQL Server currently has real data)

- [x] Inventory current production tables and row counts.
- [x] Decide migration approach:
- [x] greenfield (new database, no historical user migration), or
- [ ] scripted migration from SQL Server to PostgreSQL.
- [ ] For scripted migration, define typed mapping rules for Identity tables.
- [ ] Validate normalization/case-sensitivity behavior for usernames/emails.
- [ ] Run migration dry-run in staging.
- [x] Define rollback trigger and rollback steps before production cutover.
- [ ] Schedule low-risk cutover window and freeze writes during switch.

## Phase 4: Frontend Modernization (React + Vite)

- [x] Create a new modern frontend project (recommended path):
- [x] `LivePhotoFrame.Frontend` with React + TypeScript + Vite.
- [x] Implement selected hosting model:
- [x] ASP.NET backend serves built Vite assets as the default deployment topology.
- [x] Decommission old server-side SPA middleware in `LivePhotoFrame.ReactJs`:
- [x] remove `Microsoft.AspNetCore.SpaServices` dependency.
- [x] remove webpack/MSBuild first-run hooks tied to old toolchain.
- [ ] Add API endpoints needed by frontend and document contract.
- [x] Add frontend quality gates:
- [x] ESLint + TypeScript strict mode + Prettier.
- [x] Basic route-level test coverage (Vitest + Testing Library).
- [x] Add frontend build and preview scripts for local development.
- [ ] Retire `LivePhotoFrame.ReactJs` once parity and smoke tests pass.

## Phase 5: Native Modernization Now (.NET MAUI)

- [x] Create a MAUI app project (recommended path): `LivePhotoFrame.Maui`.
- [x] Target platforms:
- [x] `net10.0-android`
- [x] `net10.0-ios`
- [x] `net10.0-maccatalyst`
- [x] `net10.0-windows10.0.19041.0` (or higher as required)
- [x] Keep existing Xamarin/UWP projects during transition as legacy reference implementations.
- [x] Port core slideshow/domain logic into shared code usable by MAUI:
- [x] source selection (`FileSystem`, `FTP`, `HTTP/HTTPS`)
- [x] interval, max idle, image display modes, portrait skip
- [x] caching behavior and settings persistence
- [x] Add secure `HTTP/HTTPS` source provider:
- [x] `HTTPS` enabled by default
- [x] plain `HTTP` disabled by default; explicit opt-in (`AllowInsecureHttp`) for trusted LAN use
- [x] manifest/feed-driven source format (do not scrape arbitrary HTML pages)
- [x] optional auth support (`Bearer` token / API key header / signed URL pattern)
- [x] response safety controls (timeout, max file size, MIME/type checks, redirect limit)
- [x] smart network caching (`ETag`, `Last-Modified`) and retry/backoff policy
- [x] Add explicit platform services abstraction early (before deep UI work):
- [x] storage/file access adapter per platform
- [x] fullscreen/display sleep prevention adapter per platform
- [x] keyboard/gesture input adapter per platform where behavior diverges
- [x] Replace legacy Xamarin-specific dependencies where needed:
- [x] use modern MAUI/Essentials APIs for sharing, storage, and device features.
- [x] Define MAUI UI parity checklist with existing app behavior:
- [x] launch flow
- [x] settings flow
- [x] slideshow controls (next/previous/exit gestures/keys)
- [ ] Validate platform-specific behavior:
- [ ] Windows desktop
- [ ] macOS (Mac Catalyst) with dedicated quality gate:
- [ ] true fullscreen behavior and window restore
- [ ] keyboard shortcuts/navigation consistency
- [ ] sandboxed file/folder permission flow
- [ ] long-running slideshow stability and memory usage
- [ ] iOS devices/simulator
- [ ] Android devices/emulator
- [ ] Add MAUI packaging/release notes and signing prerequisites per platform.
- [x] Add explicit fallback checkpoint:
- [x] if macOS Catalyst gate fails repeatedly on UX/perf, open separate workstream for native SwiftUI macOS shell while retaining shared backend/contracts.
- [ ] Plan deprecation milestone for Xamarin/UWP projects after MAUI parity and stabilization.

## Phase 6: Security, Dependency, and Maintenance Improvements

- [x] Address vulnerable dependency graph in `LivePhotoFrame.WebApp` by moving to aligned 10.x package set.
- [x] Add dependency update automation (Dependabot or Renovate).
- [x] Add `Directory.Packages.props` for centralized package version management.
- [x] Add analyzers and warning policy for web projects.
- [x] Replace placeholder `EmailSender` with real provider integration or disable account email flows explicitly.
- [x] Add production-safe config conventions:
- [x] no secrets in `appsettings.json`
- [x] environment variable / secret store usage only.

## Phase 7: CI/CD and Quality Gates

- [x] Add GitHub Actions workflow for:
- [x] restore/build/test for `.NET 10` web projects
- [x] frontend lint/build/test for Vite app
- [x] MAUI build validation matrix (Windows runner + macOS runner as needed)
- [x] migration validation against ephemeral PostgreSQL service
- [x] Add health check endpoint and include in deployment checks.
- [x] Add minimal integration tests for auth and DB connectivity.
- [ ] Add release checklist requiring:
- [ ] migration script generated/reviewed
- [ ] backup confirmed
- [ ] rollback steps validated.

## Phase 8: Legacy Decommission and Consolidation

- [x] Reassess shared core library targeting:
- [x] keep `netstandard2.0` for maximum compatibility, or
- [ ] multi-target for modern APIs after legacy platforms are retired.
- [x] Decide retirement timeline for:
- [x] `LivePhotoFrame.Android` (Xamarin) — retire after MAUI Android parity validated.
- [x] `LivePhotoFrame.iOS` (Xamarin) — retire after MAUI iOS parity validated.
- [x] UWP variants (`LivePhotoFrame.UWP`, `LivePhotoFrame.UWPv2`) — retire after MAUI Windows parity validated.
- [x] Legacy projects opted out of central package management (`ManagePackageVersionsCentrally=false`) pending decommission.

---

## Acceptance Criteria

- [x] `LivePhotoFrame.WebApp` builds/runs on `.NET 10` with zero blocking warnings/errors.
- [x] PostgreSQL is the default provider in local/dev/prod configs.
- [x] EF migrations apply successfully on a clean PostgreSQL instance.
- [ ] Identity auth flow works end-to-end on PostgreSQL.
- [ ] Modern React + Vite frontend reaches feature parity with required user flows.
- [ ] MAUI app runs with required feature parity on Windows, iOS, and Android.
- [ ] macOS Catalyst passes quality gate; otherwise fallback workstream is approved and tracked.
- [x] `HTTP/HTTPS` source works end-to-end with secure defaults (`HTTPS` on, `HTTP` opt-in).
- [x] CI validates backend + frontend + migration checks on every PR.

## Risks and Mitigations

- [ ] Risk: Identity schema/provider edge cases between SQL Server and PostgreSQL.  
      Mitigation: separate migration set and staging validation with real auth flows.
- [ ] Risk: frontend rewrite introduces temporary feature regression.  
      Mitigation: maintain parity checklist and staged switch-over.
- [ ] Risk: mixed legacy/modern project stack slows delivery.  
      Mitigation: isolate modernization tracks and prioritize web path first.
- [ ] Risk: deployment misconfiguration during DB cutover.  
      Mitigation: pre-cutover runbook, backups, rollback trigger, and smoke checks.
- [ ] Risk: MAUI parity and platform-specific behavior drift (especially desktop vs mobile UX).  
      Mitigation: explicit parity checklist and per-platform smoke test matrix.
- [ ] Risk: Mac Catalyst can feel non-native or show edge-case desktop UX gaps.  
      Mitigation: macOS quality gate plus fallback path to native SwiftUI shell if needed.
- [ ] Risk: insecure remote image ingestion via arbitrary URLs or plain HTTP.  
      Mitigation: `HTTPS` default, `HTTP` opt-in gate, manifest model, and strict network/content validation.

## Suggested Execution Order

- [x] Wave 1 (Backend foundation): Phase 0, Phase 1, Phase 2.
- [x] Wave 2 (Data and cutover): Phase 3.
- [x] Wave 3 (Parallel tracks): Phase 4 (frontend replacement) and Phase 5 (MAUI modernization).
- [x] Wave 4 (Hardening): Phase 6 and Phase 7.
- [x] Wave 5 (Consolidation): Phase 8.

## Optional Enhancements (Nice-to-Have)

- [ ] Add OpenAPI/Swagger for API discoverability.
- [ ] Add observability baseline (structured logs + request IDs + error tracking).
- [ ] Add Docker Compose profile for local web + PostgreSQL bootstrapping.
- [ ] Add architecture diagram in `docs/architecture.md`.

## Feature Backlog (Post-Modernization)

- [ ] Smart album rotation modes:
- [ ] weighted favorites
- [ ] date-aware memory playback (On this day / monthly anniversaries)
- [ ] duplicate/near-duplicate suppression
- [ ] Burn-in and display safety improvements:
- [ ] dynamic edge shift / subtle pan-and-scan
- [ ] adaptive dimming by time schedule
- [ ] configurable blackout/sleep window
- [ ] Media and source expansion:
- [ ] S3-compatible source
- [ ] SMB/NAS source
- [ ] optional cloud providers (Google Photos, OneDrive) via plugin model
- [ ] Remote management:
- [ ] web dashboard to control playlists/settings on device
- [ ] profile sync across devices
- [ ] Security and household features:
- [ ] read-only guest mode
- [ ] optional PIN lock for settings
- [ ] encrypted credential storage hardening
