# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Smart Data Hub (SDH) — an ASP.NET Core 10 web application for managing client contact strategies ("Contactabilidad Inteligente"), built with Hexagonal Architecture (Ports & Adapters) and Domain-Driven Design (DDD).

## Build & Run

CSS must be built before or alongside the .NET app. The csproj includes a Tailwind build target that runs automatically on `dotnet build`.

```bash
# From CI_MVC/contactabilidad_inteligente.mcv/contactabilidad_inteligente.mcv/
npm install
npm run build:css        # one-time build
npm run watch:css        # watch mode during development

dotnet build             # also triggers Tailwind build
dotnet run
```

**Required environment variables:**
- `CONFIG_MASTER_KEY` — AES-256-CBC master key for decrypting connection strings and secrets in appsettings.json
- `ASPNETCORE_ENVIRONMENT` — `Development`, `Staging`, or `Production`

## Database Migrations

All EF Core commands must be run from the MVC project directory, targeting the infrastructure project:

```bash
# From CI_MVC/contactabilidad_inteligente.mcv/contactabilidad_inteligente.mcv/

# Add a new migration
dotnet ef migrations add <MigrationName> \
  -p ../../../CI_API/SDH.infrastructure \
  -s . \
  --output-dir Persistence/Migrations

# Apply migrations
dotnet ef database update -p ../../../CI_API/SDH.infrastructure -s .

# Revert all migrations
dotnet ef database update 0
```

Additional EF/DB commands are documented in `COMANDOS_UTILES.ps1` and `Generate-MigrationScript.ps1` at the root.

## Secret Management

Sensitive config values (connection strings, API keys) are AES-256-CBC encrypted. Use the CLI tool in `CI_Tools/SmartHubSecretTool/` to encrypt/decrypt values. See its README for usage. Never store plaintext secrets in appsettings.json.

## Architecture

```
CI_API/
  SDH.Domain/          # Entities, domain logic — no dependencies on other layers
  SDH.Application/     # Use cases, interfaces (ports), DTOs
  SDH.infrastructure/  # EF Core, repositories, LDAP adapter, external adapters

CI_MVC/
  contactabilidad_inteligente.mcv/   # Razor Pages presentation layer

CI_Tools/
  SmartHubSecretTool/  # CLI for encrypting appsettings secrets

CI_DB/
  SDH.SQL.DB/          # SQL Server scripts, stored procedures, Jobs
```

**Dependency flow:** Razor Pages → Application Services → Domain → Repositories (Unit of Work) → EF Core → SQL Server

## Key Patterns

**Domain entities** use private setters and static factory methods:
```csharp
var client = Cliente.Crear(nombre, rut, ...);
```

**Result pattern** — services return `Result<T>` instead of throwing for business logic failures. Only infrastructure exceptions propagate as exceptions.

**EF Core mapping** uses a hybrid strategy:
- Data Annotations on domain entities for basic column mappings
- Fluent API in `Configuration` classes (inside `SDH.infrastructure`) for indexes, foreign keys, and delete behaviors

**Unit of Work** — always commit through `IUnitOfWork`, never call `SaveChanges` directly from application services.

## Database Schema Layout

| Schema | Purpose |
|---|---|
| `seguridad` | Users, roles, authentication |
| `operativo` | Clients, contacts, officializations, financial data |
| `carga` | Staging area for Equifax imports |
| `parametro` | Configurable catalogs and enumerations |

## Authentication

Three providers configured in `Program.cs`:
1. **Local DB** — BCrypt-hashed passwords
2. **LDAP / Active Directory** — role mappings: `ADMIN`, `SUPERVISOR`, `AGENTE`, `CONSULTA`
3. **JWT** — for API access (Swagger-documented endpoints)

Authorization policies are defined in `Program.cs` and enforced via `[Authorize(Policy = "...")]` on page models.

## Coding Conventions

- PascalCase for classes, methods, and properties
- `_camelCase` prefix for private fields
- Spanish names for domain concepts (entities, properties) — this is intentional, matching the business domain language
- No test project currently exists in the solution


  
# Core Rules

**Never make git commits.** The user reviews and commits manually.

Short sentences only (8-10 words max).
No filler, no preamble, no pleasantries.
Tool first. Result first. No explain unless asked.
Code stays normal. English gets compressed.

---

## Formatting

Output sounds human. Never AI-generated.
Never use em-dashes or replacement hyphens.
Avoid parenthetical clauses entirely.
Hyphens map to standard grammar only.

---

## Approach
- Read existing files before writing. Don't re-read unless changed.
- Thorough in reasoning, concise in output.
- Skip files over 100KB unless required.
- No sycophantic openers or closing fluff.
- No emojis or em-dashes.
- Do not guess APIs, versions, flags, commit SHAs, or package names. Verify by reading code or docs before asserting.

## Deployment Documentation Rule

After any change to a configuration file (`appsettings.json`, `appsettings.*.json`, `Program.cs` logging/startup settings, environment variable definitions), ask the user: "¿En qué documento de pase debo registrar este cambio?" before closing the task. Wait for the user to specify the target file under `docs/Publicacion_Pro/` and update it accordingly. Never skip this step — undocumented config changes break future deployments.