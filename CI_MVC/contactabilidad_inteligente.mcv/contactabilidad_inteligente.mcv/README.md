# Contactabilidad Inteligente (Razor Pages)

Proyecto Razor Pages en .NET 9 que implementa una arquitectura Hexagonal (Ports & Adapters) y principios de Clean Code y DDD. Proporciona entidades y mapeos EF Core para los esquemas: `seguridad`, `operativo`, `carga` y `parametro`.

## Estructura principal

- `Domain/` - Entidades de dominio (Clientes, Contactos, Financiero, Usuarios, Cat�logos, etc.).
- `Application/` - Puertos (interfaces) y servicios de aplicaci�n (use-cases).
- `Infrastructure/Persistence/` - Implementaciones con EF Core: configuraciones Fluent, repositorios y UnitOfWork.
- `Data/ApplicationDbContext.cs` - `DbContext` central con `DbSet<>` y carga de configuraciones.
- `Pages/` - Razor Pages de la UI (ej. `Clientes`).
- `ARCHITECTURE.md` - Documentaci�n detallada de la implementaci�n y decisiones de arquitectura.

## Caracter�sticas principales

- .NET 9 / C# 13
- Razor Pages
- EF Core (SQL Server) con **Data Annotations** (atributos) en las entidades del dominio
- Fluent API solo para configuraciones avanzadas (�ndices, relaciones con DeleteBehavior, valores por defecto)
- Repositorios e interfaces (Ports) para separar dominio e infraestructura
- Unit of Work para manejo de transacciones
- Servicios de aplicaci�n (Application Services) que usan los puertos
- Patrones: Repository, Unit of Work, Result Pattern, Factory Methods

## Estrategia de mapeo EF Core

### Data Annotations (en entidades)
Las entidades del dominio usan Data Annotations para:
- ? Mapeo de tablas y esquemas (`[Table]`, `[Column]`)
- ? Definici�n de claves primarias (`[Key]`)
- ? Tipos de generaci�n de valores (`[DatabaseGenerated]`)
- ? Restricciones de longitud (`[MaxLength]`)
- ? Campos requeridos (`[Required]`)
- ? Tipos de datos SQL (`TypeName` en `[Column]`)
- ? Relaciones b�sicas (`[ForeignKey]`)

### Fluent API (en configuraciones)
Las clases de configuraci�n (`IEntityTypeConfiguration<T>`) usan Fluent API **solo** para:
- ? �ndices simples y compuestos
- ? �ndices �nicos
- ? Comportamiento de eliminaci�n en relaciones (`DeleteBehavior.Restrict`)
- ? Nombres de constraints y foreign keys
- ? Valores por defecto de base de datos (`HasDefaultValue`, `HasDefaultValueSql`)

Esta estrategia mantiene las entidades autodescriptivas y reduce la configuraci�n externa al m�nimo necesario.

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

Las entidades y configuraciones est�n dise�adas para reflejar las columnas y relaciones indicadas en los scripts SQL base.

## Requisitos

- .NET 9 SDK
- SQL Server (o LocalDB)

## Configuraci�n y ejecuci�n

1. Configurar la cadena de conexi�n en `appsettings.json` bajo `ConnectionStrings:DefaultConnection`:

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

3. Crear la migraci�n inicial y aplicar al servidor de base de datos:

```bash
dotnet ef migrations add InitialCreate -p contactabilidad_inteligente.mcv -s contactabilidad_inteligente.mcv --output-dir Data/Migrations
dotnet ef database update -p contactabilidad_inteligente.mcv -s contactabilidad_inteligente.mcv
```

> Nota: Ajustar los par�metros `-p` y `-s` si ejecutas desde la ra�z del repositorio.

4. Ejecutar la aplicaci�n:

```bash
dotnet run --project contactabilidad_inteligente.mcv
```

Abrir el navegador en `https://localhost:5001/Clientes` (la aplicaci�n redirige a `/login` por defecto).

## Dependencias y DI

- Las implementaciones de persistencia y servicios de aplicaci�n se registran en `Program.cs` mediante las extensiones `AddPersistence()` y `AddApplicationServices()`.
- Repositorios disponibles: `IClienteRepository`, `IUsuarioRepository`, `ICatalogoRepository`.
- `IUnitOfWork` proporciona transacciones y acceso a repositorios.

## Buenas pr�cticas / pr�ximos pasos

- Revisar y ajustar `ARCHITECTURE.md` para pol�ticas de despliegue y pruebas.
- Agregar pruebas unitarias para servicios y repositorios (usar `InMemory` o `Sqlite` en memoria para EF Core).
- A�adir validaciones y manejo de errores m�s detallado en la capa de UI.
- Considerar migraciones incrementalmente por cambios en el dominio.

## Ventajas del enfoque Data Annotations + Fluent API

1. **Entidades autodescriptivas**: El c�digo de dominio contiene su propia metadata
2. **Menos archivos de configuraci�n**: Solo se crean configuraciones para casos especiales
3. **Mejor IntelliSense**: Los atributos son visibles directamente en las propiedades
4. **Facilita el Code-First**: Las migraciones se generan directamente desde las entidades
5. **Clean Code**: Separaci�n clara entre configuraciones simples (annotations) y complejas (fluent)

## Archivo de referencia

- `ARCHITECTURE.md` � Documentaci�n de dise�o y decisiones arquitect�nicas.

---

**Compilaci�n verificada**: ? Exitosa

//"DefaultConnection": "Server=192.168.250.109;Database=DB_ODS;User ID=tu_usuario;Password=tu_contraseña;Persist Security Info=False;TrustServerCertificate=True;"
