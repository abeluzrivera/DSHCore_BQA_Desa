# Contactabilidad Inteligente (Razor Pages)

Proyecto Razor Pages en .NET 9 que implementa una arquitectura Hexagonal (Ports & Adapters) y principios de Clean Code y DDD. Proporciona entidades y mapeos EF Core para los esquemas: `seguridad`, `operativo`, `carga` y `parametro`.

## Estructura principal

- `Domain/` - Entidades de dominio (Clientes, Contactos, Financiero, Usuarios, Catálogos, etc.).
- `Application/` - Puertos (interfaces) y servicios de aplicación (use-cases).
- `Infrastructure/Persistence/` - Implementaciones con EF Core: configuraciones Fluent, repositorios y UnitOfWork.
- `Data/ApplicationDbContext.cs` - `DbContext` central con `DbSet<>` y carga de configuraciones.
- `Pages/` - Razor Pages de la UI (ej. `Clientes`).
- `ARCHITECTURE.md` - Documentación detallada de la implementación y decisiones de arquitectura.

## Características principales

- .NET 9 / C# 13
- Razor Pages
- EF Core (SQL Server) con **Data Annotations** (atributos) en las entidades del dominio
- Fluent API solo para configuraciones avanzadas (índices, relaciones con DeleteBehavior, valores por defecto)
- Repositorios e interfaces (Ports) para separar dominio e infraestructura
- Unit of Work para manejo de transacciones
- Servicios de aplicación (Application Services) que usan los puertos
- Patrones: Repository, Unit of Work, Result Pattern, Factory Methods

## Estrategia de mapeo EF Core

### Data Annotations (en entidades)
Las entidades del dominio usan Data Annotations para:
- ? Mapeo de tablas y esquemas (`[Table]`, `[Column]`)
- ? Definición de claves primarias (`[Key]`)
- ? Tipos de generación de valores (`[DatabaseGenerated]`)
- ? Restricciones de longitud (`[MaxLength]`)
- ? Campos requeridos (`[Required]`)
- ? Tipos de datos SQL (`TypeName` en `[Column]`)
- ? Relaciones básicas (`[ForeignKey]`)

### Fluent API (en configuraciones)
Las clases de configuración (`IEntityTypeConfiguration<T>`) usan Fluent API **solo** para:
- ? Índices simples y compuestos
- ? Índices únicos
- ? Comportamiento de eliminación en relaciones (`DeleteBehavior.Restrict`)
- ? Nombres de constraints y foreign keys
- ? Valores por defecto de base de datos (`HasDefaultValue`, `HasDefaultValueSql`)

Esta estrategia mantiene las entidades autodescriptivas y reduce la configuración externa al mínimo necesario.

## Esquemas y tablas mapeadas

- `seguridad`:
  - `Tbl_Maest_Usuario`

- `operativo`:
  - `Tbl_Maest_Cliente`
  - `Tbl_Traz_Financiero_cliente`
  - `Tbl_Traz_Contacto_Cliente`
  - `Tbl_Traz_Oficializacion_Core`

- `carga`:
  - `Tbl_Stg_Equifax_Importacion`

- `parametro`:
  - `Tbl_Cat_Grupo`
  - `Tbl_Cat_Item`

Las entidades y configuraciones están diseñadas para reflejar las columnas y relaciones indicadas en los scripts SQL base.

## Requisitos

- .NET 9 SDK
- SQL Server (o LocalDB)

## Configuración y ejecución

1. Configurar la cadena de conexión en `appsettings.json` bajo `ConnectionStrings:DefaultConnection`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ContactabilidadInteligente;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

2. Instalar herramientas EF Core si es necesario:

```bash
dotnet tool install --global dotnet-ef
```

3. Crear la migración inicial y aplicar al servidor de base de datos:

```bash
dotnet ef migrations add InitialCreate -p contactabilidad_inteligente.mcv -s contactabilidad_inteligente.mcv --output-dir Data/Migrations
dotnet ef database update -p contactabilidad_inteligente.mcv -s contactabilidad_inteligente.mcv
```

> Nota: Ajustar los parámetros `-p` y `-s` si ejecutas desde la raíz del repositorio.

4. Ejecutar la aplicación:

```bash
dotnet run --project contactabilidad_inteligente.mcv
```

Abrir el navegador en `https://localhost:5001/Clientes` (la aplicación redirige a `/login` por defecto).

## Dependencias y DI

- Las implementaciones de persistencia y servicios de aplicación se registran en `Program.cs` mediante las extensiones `AddPersistence()` y `AddApplicationServices()`.
- Repositorios disponibles: `IClienteRepository`, `IUsuarioRepository`, `ICatalogoRepository`.
- `IUnitOfWork` proporciona transacciones y acceso a repositorios.

## Buenas prácticas / próximos pasos

- Revisar y ajustar `ARCHITECTURE.md` para políticas de despliegue y pruebas.
- Agregar pruebas unitarias para servicios y repositorios (usar `InMemory` o `Sqlite` en memoria para EF Core).
- Añadir validaciones y manejo de errores más detallado en la capa de UI.
- Considerar migraciones incrementalmente por cambios en el dominio.

## Ventajas del enfoque Data Annotations + Fluent API

1. **Entidades autodescriptivas**: El código de dominio contiene su propia metadata
2. **Menos archivos de configuración**: Solo se crean configuraciones para casos especiales
3. **Mejor IntelliSense**: Los atributos son visibles directamente en las propiedades
4. **Facilita el Code-First**: Las migraciones se generan directamente desde las entidades
5. **Clean Code**: Separación clara entre configuraciones simples (annotations) y complejas (fluent)

## Archivo de referencia

- `ARCHITECTURE.md` – Documentación de diseño y decisiones arquitectónicas.

---

**Compilación verificada**: ? Exitosa