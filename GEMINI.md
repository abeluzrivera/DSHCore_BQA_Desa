# Project: bm_contactabilidad_inteligente (Smart Contactability)

Foundational mandates and project-specific intelligence for Gemini CLI.

## 🎯 Project Overview
This is a .NET 9 Razor Pages application designed for managing smart contactability. It utilizes a Hexagonal Architecture (Ports & Adapters) combined with Domain-Driven Design (DDD) and Clean Code principles to ensure a decoupled, maintainable, and testable system.

## 🏗️ Architectural Mandates

### Layers (Hexagonal / Clean Architecture)
- **Domain Layer (`CI_API/SDH.Domain`):** The core of the application. Contains entities, value objects, and domain logic. Entities must use private setters and Factory Methods for creation.
- **Application Layer (`CI_API/SDH.Application`):** Contains "Ports" (interfaces) and Application Services (use cases). It coordinates the flow of data between the domain and infrastructure.
- **Infrastructure Layer (`CI_API/SDH.infrastructure`):** Contains "Adapters" (concrete implementations). This includes EF Core persistence, external service integrations, and repository implementations.
- **Presentation Layer (`CI_MVC/...`):** Razor Pages UI that consumes the Application Layer.

### Data Persistence (EF Core)
- **Hybrid Mapping Strategy:** 
    - **Data Annotations:** Use within Domain Entities for primary keys, lengths, required fields, and table/schema mapping (`seguridad`, `operativo`, `carga`, `parametro`).
    - **Fluent API:** Use strictly in `Infrastructure/Persistence/Configurations` for complex requirements: composite/unique indexes, `DeleteBehavior.Restrict`, and database default values.
- **Patterns:** Repository and Unit of Work (UoW) must be used for all database operations.

## 🛠️ Technical Stack
- **Runtime:** .NET 9 / C# 13
- **UI:** Razor Pages with Tailwind CSS (Build pipeline via `npm run build:css`).
- **ORM:** Entity Framework Core 9 (SQL Server).
- **Libraries:**
    - `BCrypt.Net-Next`: For password hashing.
    - `EPPlus`: For Excel file processing.
    - `Blazorise`: For specific UI components.

## 📜 Coding Standards
- **Naming:** Follow standard .NET PascalCase for classes/methods and camelCase for private fields (prefixed with `_`).
- **Encapsulation:** Never expose public setters on Domain Entities. Use domain methods (e.g., `UpdateStatus`) to modify state.
- **Result Pattern:** Use a Result object for service responses to avoid throwing exceptions for expected business logic failures.

## 🚀 Critical Workflows

### Database Migrations
Always run migrations from the MVC project directory, specifying the infrastructure project as the source:
```powershell
dotnet ef migrations add <MigrationName> -p CI_API\SDH.infrastructure -s CI_MVC\contactabilidad_inteligente.mcv\contactabilidad_inteligente.mcv --output-dir Persistence\Migrations
dotnet ef database update -p CI_API\SDH.infrastructure -s CI_MVC\contactabilidad_inteligente.mcv\contactabilidad_inteligente.mcv
```

### Build & Run
Ensure Tailwind CSS is built before running:
```powershell
# Inside CI_MVC\contactabilidad_inteligente.mcv\contactabilidad_inteligente.mcv
npm install
npm run build:css
dotnet run
```

## 📂 Key Directory Map
- `CI_API/SDH.Domain/Entities`: Core business models organized by schema.
- `CI_API/SDH.Application/Ports`: Repository and service interfaces.
- `CI_API/SDH.infrastructure/Persistence/Configurations`: Fluent API mappings.
- `CI_MVC/contactabilidad_inteligente.mcv/contactabilidad_inteligente.mcv/Pages`: Razor Pages UI.
