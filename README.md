# Contactabilidad Inteligente (Smart Contactability)

Sistema de gestión de contactabilidad de clientes desarrollado con **ASP.NET Core 10**, siguiendo **Arquitectura Hexagonal (Ports & Adapters)**, **Domain-Driven Design (DDD)** y **Clean Code**.

## Tecnologías

| Capa | Tecnología |
|---|---|
| Framework | ASP.NET Core 10 / C# 13 |
| UI | Razor Pages + Tailwind CSS |
| Persistencia | EF Core 10, SQL Server |
| Autenticación | Cookies + LDAP/AD + JWT |
| Logging | Serilog (archivo + consola) |
| Secretos | AES-256-CBC (herramienta interna) |
| Observabilidad | Microsoft Clarity |

## Estructura del Proyecto

```
DSHCore_BQA_Desa/
├── CI_API/
│   ├── SDH.Domain/          # Entidades, value objects, lógica de dominio
│   ├── SDH.Application/     # Puertos (interfaces), DTOs, casos de uso
│   └── SDH.infrastructure/  # EF Core, repositorios, LDAP, caché, servicios externos
│
├── CI_MVC/
│   └── contactabilidad_inteligente.mcv/   # Razor Pages (presentación)
│
├── CI_Tools/
│   └── SmartHubSecretTool/  # CLI para cifrar/descifrar valores en appsettings
│
└── CI_DB/
    └── SDH.SQL.DB/          # Scripts SQL, procedimientos almacenados, Jobs
```

**Flujo de dependencias:** Razor Pages → Application Services → Domain → Repositories (Unit of Work) → EF Core → SQL Server

## Esquema de Base de Datos

| Schema | Propósito |
|---|---|
| `seguridad` | Usuarios, roles, autenticación local |
| `operativo` | Clientes, contactos, oficializaciones, datos financieros |
| `carga` | Staging para importaciones Equifax |
| `parametro` | Catálogos y enumeraciones configurables |

## Configuración Inicial

### Requisitos

- .NET 10 SDK
- SQL Server 2019+ (o LocalDB)
- Node.js 20+ y npm

### Variables de Entorno Requeridas

| Variable | Descripción |
|---|---|
| `CONFIG_MASTER_KEY` | Clave AES-256-CBC para descifrar cadenas de conexión y secretos en appsettings |
| `JwtSettings__SecretKey` | Clave de firma para tokens JWT (texto plano, mínimo 32 caracteres) |
| `ASPNETCORE_ENVIRONMENT` | `Development`, `Staging` o `Production` |

> Las variables de entorno deben definirse a nivel de sistema. En Windows Server con IIS, configurarlas en las Variables de Entorno del sistema y reiniciar IIS para que el proceso las herede.

### Instalación

1. **Clonar y restaurar dependencias de UI:**
   ```bash
   cd CI_MVC/contactabilidad_inteligente.mcv/contactabilidad_inteligente.mcv
   npm install
   ```

2. **Configurar la cadena de conexión** en `appsettings.json` usando la herramienta de cifrado:
   ```bash
   cd CI_Tools/SmartHubSecretTool/SDH.SecretTool
   dotnet run -- encrypt "Server=TU_SERVIDOR;Database=CI_DB;..."
   ```
   Reemplazar el valor resultante (`ENC:...`) en la propiedad `ConnectionStrings:DefaultConnection`.

3. **Aplicar migraciones** (desde el directorio del proyecto MVC):
   ```bash
   cd CI_MVC/contactabilidad_inteligente.mcv/contactabilidad_inteligente.mcv
   dotnet ef database update -p ../../../CI_API/SDH.infrastructure -s .
   ```

## Comandos de Desarrollo

Ejecutar desde `CI_MVC/contactabilidad_inteligente.mcv/contactabilidad_inteligente.mcv/`:

```bash
# Compilar CSS (una vez)
npm run build:css

# Modo watch durante desarrollo
npm run watch:css

# Ejecutar la aplicación (incluye build de Tailwind automáticamente)
dotnet run
```

### Migraciones EF Core

```bash
# Nueva migración
dotnet ef migrations add <NombreMigracion> \
  -p ../../../CI_API/SDH.infrastructure \
  -s . \
  --output-dir Persistence/Migrations

# Revertir todas las migraciones
dotnet ef database update 0
```

Ver también `COMANDOS_UTILES.ps1` y `Generate-MigrationScript.ps1` en la raíz para comandos adicionales.

## Autenticación

Tres proveedores configurados en `Program.cs`:

1. **Base de datos local** — contraseñas hasheadas con BCrypt
2. **LDAP / Active Directory** — mapeo de grupos a roles: `ADMIN`, `SUPERVISOR`, `AGENTE`, `CONSULTA`
3. **JWT** — para endpoints API documentados en Swagger

Las políticas de autorización se definen en `Program.cs` y se aplican con `[Authorize(Policy = "...")]`.

## Gestión de Secretos

Los valores sensibles en `appsettings.json` se cifran con AES-256-CBC usando la herramienta en `CI_Tools/SmartHubSecretTool/`. Los valores cifrados tienen el prefijo `ENC:`. Ver el README de la herramienta para instrucciones de uso.

**No almacenar secretos en texto plano en ningún archivo de configuración.**

## Patrones de Implementación

### Entidades de Dominio

Setters privados e instanciación por factory method:
```csharp
var cliente = Cliente.Crear(nombre, rut, ...);
```

### Result Pattern

Los servicios de aplicación retornan `Result<T>` en lugar de lanzar excepciones para fallos de negocio. Solo las excepciones de infraestructura se propagan.

### Mapeo EF Core

- **Data Annotations** en las entidades para mapeos básicos (columnas, longitudes)
- **Fluent API** en clases `Configuration` dentro de `SDH.infrastructure` para índices, claves foráneas y comportamientos de borrado

### Unit of Work

Siempre persistir a través de `IUnitOfWork`. No llamar `SaveChanges` directamente desde servicios de aplicación.

## Design System (DS)

El Design System cubre tokens (colores, tipografía, espaciado), componentes CSS (botones, cards, tablas, formularios, modals) y módulos JS reutilizables.

Referencia completa: [`docs/Tecnica/DESIGN_SYSTEM.md`](docs/Tecnica/DESIGN_SYSTEM.md)

| Módulo JS | Responsabilidad |
|---|---|
| `ds-core.js` | EventBus, utilidades, estado reactivo, registro de módulos |
| `ds-api.js` | Wrapper de Axios con interceptores de autenticación y errores |
| `ds-endpoints.js` | Centralización de URLs de la API REST |
| `ds-search.js` | Widget de búsqueda global con dropdown de sugerencias |
| `ds-notifications.js` | Sistema de toasts |
| `ds-client-list-store.js` | Persistencia de la lista de clientes en sessionStorage (TTL 8h) |
| `ds-catalogs.js` | Catálogos de tipos de contacto y estados |
| `ds-modal.js` | Gestión de modales |

La comunicación entre módulos se realiza a través de `DS.events` (pub/sub desacoplado).

## Documentación

| Documento | Ubicación | Contenido |
|---|---|---|
| Design System | `docs/Tecnica/DESIGN_SYSTEM.md` | Tokens CSS, componentes, módulos JS, patrones de uso |
| Runbook | `docs/Tecnica/Runbook.md` | Operación, deploy, incidentes, contactos |
| Manual IIS | `docs/Tecnica/Manual_implementacion_IIS.md` | Instalación paso a paso en Windows Server |
| Procedimientos | `docs/Funcional/Procedimientos.md` | Flujos de negocio y control de acceso |
| Arquitectura | `docs/Archi/` | Diagramas ArchiMate |
| Assets | `docs/assets/` | Logo, imágenes y recursos compartidos |
