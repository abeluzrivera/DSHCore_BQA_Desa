# Plan de Trabajo — Deuda Técnica Items 1 a 5
## Correcciones de Esquema, Dominio y Configuración EF Core
### Release 1.1.0 — Paquete A

**Fecha:** 05 de junio de 2026  
**Estado:** Planificado  
**Responsable:** Por asignar  
**Estimación:** 2-3 días de desarrollo + revisión

---

## Objetivo

Corregir cinco inconsistencias de nomenclatura y configuración que afectan la legibilidad del dominio y la consistencia entre el modelo EF Core y el esquema real de base de datos. Ninguno de estos cambios altera comportamiento observable desde la UI; todos son correcciones internas de consistencia.

---

## Resumen de Items

| # | Item | Tipo de cambio | Requiere migración EF |
|---|------|----------------|-----------------------|
| DT-01 | Renombrar `ClientFinancial.ClientId` → `AccountingType` | Código | No |
| DT-02 | Parametrizar `CustomerAddresses.Source` | Código | No |
| DT-03 | Corregir `HasMaxLength(32)` → `20` en `ClienteConfiguration` | Código + migración | Sí |
| DT-04 | Renombrar índice `IX_Contacto_Cliente_*` → `IX_Direccion_Cliente_*` | Código + migración | Sí |
| DT-05 | Renombrar columna `estado_LOPDP` → `Estado_LOPDP` | Código + migración | Sí |

Los items DT-03, DT-04 y DT-05 deben agruparse en una sola migración EF Core para minimizar el número de ALTER TABLE ejecutados en base de datos.

---

## DT-01 — Renombrar `ClientFinancial.ClientId` a `AccountingType`

### Estado actual

La entidad `ClientFinancial` tiene una propiedad `ClientId` de tipo `string` que almacena el tipo de contabilidad (`Tipo_Contabilidad`). El nombre es engañoso: `ClientId` sugiere un identificador de cliente cuando en realidad es un campo descriptivo del tipo contable.

```
Archivo: CI_API/SDH.Domain/Entities/Operative/ClientFinancial.cs
```

Propiedades y métodos afectados en la entidad:
- `public string ClientId { get; private set; }` — propiedad a renombrar
- `Create(...)`: asigna `ClientId = tipoContabilidad`
- `UpdateType(string nuevoTipo)`: asigna `ClientId = nuevoTipo`

```
Archivo: CI_API/SDH.infrastructure/Persistence/DataConfigurations/Operativo/FinancieroClienteConfiguration.cs
```

Referencias en la configuración EF:
- `builder.Property(f => f.ClientId).HasColumnName("Tipo_Contabilidad")...`
- `builder.HasIndex(f => f.ClientId).HasDatabaseName("IX_FinancieroCliente_TipoContabilidad")`

La columna SQL (`Tipo_Contabilidad`) y el índice de base de datos (`IX_FinancieroCliente_TipoContabilidad`) **no cambian**. Solo cambia el nombre de la propiedad C# y las lambdas de configuración. No se requiere migración EF.

### Archivos afectados

| Archivo | Cambio |
|---------|--------|
| `CI_API/SDH.Domain/Entities/Operative/ClientFinancial.cs` | Renombrar propiedad y actualizar cuerpo de `Create()` y `UpdateType()` |
| `CI_API/SDH.infrastructure/Persistence/DataConfigurations/Operativo/FinancieroClienteConfiguration.cs` | Actualizar lambdas `f.ClientId` → `f.AccountingType` (2 ocurrencias) |
| `CI_API/SDH.Application/Services/CustomerCommandService.cs` | Verificar si accede a `.ClientId` directamente; actualizar si existe |
| `CI_API/SDH.Domain/Entities/Operative/Client.cs` | Verificar si proyecta o accede a `ClientFinancial.ClientId` |
| `CI_API/SDH.infrastructure/Persistence/Data/ApplicationDbContext.cs` | Verificar si hay referencias directas a la propiedad |

### Pasos de implementación

1. Abrir `ClientFinancial.cs`.
2. Renombrar la propiedad `ClientId` a `AccountingType`.
3. En el método `Create()`, cambiar `ClientId = tipoContabilidad` a `AccountingType = tipoContabilidad`.
4. En el método `UpdateType()`, cambiar `ClientId = nuevoTipo` a `AccountingType = nuevoTipo`.
5. Abrir `FinancieroClienteConfiguration.cs`.
6. Cambiar `builder.Property(f => f.ClientId)` a `builder.Property(f => f.AccountingType)`.
7. Cambiar `builder.HasIndex(f => f.ClientId)` a `builder.HasIndex(f => f.AccountingType)`.
8. Compilar la solución. Resolver cualquier error de compilación navegando a los archivos que el compilador señale con referencia a `ClientId` en el contexto de `ClientFinancial`.
9. Verificar que no haya consultas LINQ en servicios o repositorios que proyecten o filtren por `clientFinancial.ClientId`.

### Resultado esperado

La propiedad pasa a llamarse `AccountingType`, lo que hace evidente que es el tipo contable. La columna `Tipo_Contabilidad` y el índice en base de datos no se modifican. No se genera ninguna migración EF.

### Criterios de aceptación

- La solución compila sin errores.
- No existe ninguna referencia a `ClientFinancial.ClientId` en el codebase tras el cambio.
- El snapshot de EF Core no registra diferencias al ejecutar `dotnet ef migrations add` en modo de verificación (sin guardar).

---

## DT-02 — Parametrizar `CustomerAddresses.Source`

### Estado actual

El factory method `CustomerAddresses.Create()` tiene el origen hardcodeado:

```csharp
Source = GlobalVariables.SystemUser,
```

Esto impide identificar el origen real de la dirección (operador humano, carga masiva, sistema). La entidad equivalente `CustomerContacts` ya recibe el origen como parámetro mediante `CustomerContacts.Create(..., string source)`.

```
Archivo: CI_API/SDH.Domain/Entities/Operative/CustomerAddresses.cs
```

### Archivos afectados

| Archivo | Cambio |
|---------|--------|
| `CI_API/SDH.Domain/Entities/Operative/CustomerAddresses.cs` | Agregar parámetro `string source` al método `Create()` |
| `CI_API/SDH.Application/Services/CustomerCommandService.cs` | Actualizar todas las llamadas a `CustomerAddresses.Create()` para pasar el origen |

### Pasos de implementación

1. Antes de modificar, ejecutar búsqueda de todas las llamadas a `CustomerAddresses.Create(` en la solución para registrar los puntos de uso:
   ```
   grep -rn "CustomerAddresses.Create(" D:\sources\DSHCore_BQA_Desa\CI_API
   grep -rn "CustomerAddresses.Create(" D:\sources\DSHCore_BQA_Desa\CI_MVC
   ```
2. Abrir `CustomerAddresses.cs`.
3. Agregar el parámetro `string source` al final de la firma de `Create()`, antes de `string? usuarioCreacion`:
   ```csharp
   public static CustomerAddresses Create(
       int? tipoDireccion,
       string? direccionCompleta,
       string? ciudad,
       string? provincia,
       string? codigoPostal,
       string? pais,
       string? codigoPais,
       string? codigoCiudad,
       string? codigoProvincia,
       decimal? latitud,
       decimal? longitud,
       bool esPrincipal,
       string source,           // ← nuevo parámetro
       string? usuarioCreacion)
   ```
4. En el cuerpo del método, reemplazar `Source = GlobalVariables.SystemUser` por `Source = source`.
5. Actualizar `CustomerCommandService.cs`: en cada llamada a `CustomerAddresses.Create()`, pasar el valor de origen correspondiente. Para operaciones manuales del operador, pasar el identificador del usuario activo. Para procesos de carga masiva, pasar una constante identificadora del proceso batch (por ejemplo, `GlobalVariables.BulkUploadSource` o el identificador del archivo).
6. Si existe algún otro caller fuera de `CustomerCommandService`, actualizarlo de la misma forma.
7. Compilar y resolver errores.

### Consideraciones

- Revisar si `CustomerContacts.Create()` usa un tipo `string` simple o una constante/enum para el source. Mantener consistencia con ese patrón.
- Si en el futuro se requiere un catálogo de orígenes, el parámetro ya es un `string` sin acoplamiento a un enum específico. No anticipar esa estructura ahora.

### Criterios de aceptación

- El método `CustomerAddresses.Create()` acepta `source` como parámetro.
- No existe ninguna asignación `Source = GlobalVariables.SystemUser` dentro del factory method de `CustomerAddresses`.
- Todos los callers existentes compilan y pasan un origen semánticamente correcto.

---

## DT-03 — Corregir `HasMaxLength(32)` a `20` en `ClienteConfiguration`

### Estado actual

La configuración EF Core de la entidad `Client` define `Identificacion_Cliente` con `HasMaxLength(32)`, pero la columna SQL está definida como `NVARCHAR(20)`.

```
Archivo: CI_API/SDH.infrastructure/Persistence/DataConfigurations/Operativo/ClienteConfiguration.cs
Línea: 27
```

Código actual:
```csharp
builder.Property(c => c.Identification)
       .HasColumnName("Identificacion_Cliente")
       .HasMaxLength(32)   // ← incorrecto
       .IsRequired();
```

Este desajuste provoca que EF Core genere una diferencia en `dotnet ef migrations add` (detecta la columna como modificada), y permite que el modelo de dominio acepte valores de hasta 32 caracteres que la base de datos rechazaría.

### Archivos afectados

| Archivo | Cambio |
|---------|--------|
| `CI_API/SDH.infrastructure/Persistence/DataConfigurations/Operativo/ClienteConfiguration.cs` | `HasMaxLength(32)` → `HasMaxLength(20)` en línea 27 |
| Migración EF nueva | `AlterColumn` para sincronizar el modelo con la columna real |

### Pasos de implementación

1. Abrir `ClienteConfiguration.cs`, línea 27.
2. Cambiar `HasMaxLength(32)` a `HasMaxLength(20)`.
3. Verificar si la entidad `Client.cs` tiene alguna anotación `[MaxLength(32)]` o `[StringLength(32)]` sobre la propiedad `Identification`. Si la tiene, corregirla también a 20.
4. Esta corrección se incluye en la migración consolidada descrita al final de este documento (sección Migración Consolidada DT-03/04/05).

### Consideraciones

- La columna SQL ya es `NVARCHAR(20)`. La migración generará un `AlterColumn` que EF aplicará con `nvarchar(20)`. Si la base de datos ya tiene la columna en ese tamaño, SQL Server ejecutará el comando sin mover datos.
- Verificar que no exista ningún cliente registrado con `Identificacion_Cliente` de más de 20 caracteres antes de ejecutar la migración en producción: `SELECT MAX(LEN(Identificacion_Cliente)) FROM operativo.Tbl_Maest_Cliente`.

### Criterios de aceptación

- `HasMaxLength(20)` en `ClienteConfiguration.cs`.
- Al ejecutar `dotnet ef migrations add VerificacionDT` no aparece diferencia para la columna `Identificacion_Cliente`.
- La migración consolidada se aplica sin errores en el ambiente de prueba.

---

## DT-04 — Renombrar índice `IX_Contacto_Cliente_IdCliente_EstaEliminado`

### Estado actual

El índice definido sobre la tabla `Tbl_Direccion_Cliente` lleva en su nombre la palabra "Contacto", que corresponde a una tabla diferente. El nombre correcto debe referenciar "Direccion".

```
Archivo: CI_API/SDH.infrastructure/Persistence/DataConfigurations/Operativo/CustomerAddressConfiguration.cs
Línea: 153
```

Código actual:
```csharp
builder.HasIndex(d => new { d.ClientId, d.IsDeleted })
    .HasDatabaseName("IX_Contacto_Cliente_IdCliente_EstaEliminado");
```

El índice ya existe en base de datos con el nombre incorrecto (fue creado en la migración `20260331153327_InitCreate`). Se requiere una migración para renombrarlo.

### Archivos afectados

| Archivo | Cambio |
|---------|--------|
| `CI_API/SDH.infrastructure/Persistence/DataConfigurations/Operativo/CustomerAddressConfiguration.cs` | Actualizar `HasDatabaseName` al nombre correcto |
| Migración EF nueva | `RenameIndex` o drop + recreate del índice |

### Pasos de implementación

1. Abrir `CustomerAddressConfiguration.cs`, línea 153.
2. Cambiar `"IX_Contacto_Cliente_IdCliente_EstaEliminado"` a `"IX_Direccion_Cliente_IdCliente_EstaEliminado"`.
3. Esta corrección se incluye en la migración consolidada (sección Migración Consolidada DT-03/04/05).
4. En la migración, el cambio de nombre de un índice en SQL Server se hace mediante drop + recreate porque `sp_rename` no es compatible con EF Core Migrations de forma portable:
   ```csharp
   migrationBuilder.DropIndex(
       name: "IX_Contacto_Cliente_IdCliente_EstaEliminado",
       table: "Tbl_Direccion_Cliente",
       schema: "operativo");
   
   migrationBuilder.CreateIndex(
       name: "IX_Direccion_Cliente_IdCliente_EstaEliminado",
       table: "Tbl_Direccion_Cliente",
       schema: "operativo",
       columns: new[] { "Id_Cliente", "Esta_Eliminado" });
   ```

### Consideraciones

- Si se prefiere usar `sp_rename` para evitar el bloqueo de tabla durante drop + recreate en producción, hacerlo directamente en SQL fuera de la migración y luego actualizar solo la configuración EF. El snapshot debe reflejar el nuevo nombre.
- Para tablas con alto volumen de filas, el drop + recreate de un índice no clustered puede generar bloqueo momentáneo. Evaluar ejecutar en ventana de mantenimiento.

### Criterios de aceptación

- `CustomerAddressConfiguration.cs` referencia `IX_Direccion_Cliente_IdCliente_EstaEliminado`.
- No existe ningún índice llamado `IX_Contacto_Cliente_IdCliente_EstaEliminado` en la base de datos después de aplicar la migración.
- El nuevo índice `IX_Direccion_Cliente_IdCliente_EstaEliminado` existe y es funcional.

---

## DT-05 — Renombrar columna `estado_LOPDP` a `Estado_LOPDP`

### Estado actual

La columna `estado_LOPDP` en la tabla `operativo.Tbl_Direccion_Cliente` está en minúsculas, violando la convención `Snake_Pascal_Case` usada en todo el esquema (ejemplos correctos: `Esta_Verificado`, `Esta_Eliminado`, `Fecha_Creacion`).

```
Archivo: CI_API/SDH.infrastructure/Persistence/DataConfigurations/Operativo/CustomerAddressConfiguration.cs
```

Código actual:
```csharp
builder.Property(d => d.LopdpStatus)
       .HasColumnName("estado_LOPDP")
       .HasMaxLength(20);
```

La columna fue creada en la migración `20260331153327_InitCreate` con el nombre incorrecto. El snapshot y el archivo de migración también reflejan `"estado_LOPDP"`.

### Archivos afectados

| Archivo | Cambio |
|---------|--------|
| `CI_API/SDH.infrastructure/Persistence/DataConfigurations/Operativo/CustomerAddressConfiguration.cs` | `HasColumnName("estado_LOPDP")` → `HasColumnName("Estado_LOPDP")` |
| `CI_API/SDH.infrastructure/Migrations/ApplicationDbContextModelSnapshot.cs` | Se actualiza automáticamente al generar la migración |
| Migración EF nueva | `RenameColumn` de `estado_LOPDP` a `Estado_LOPDP` |

No es necesario modificar los archivos de la migración original `20260331153327_InitCreate`. La nueva migración aplica el renombrado sobre el estado actual de la base de datos.

### Pasos de implementación

1. Abrir `CustomerAddressConfiguration.cs`.
2. Cambiar `HasColumnName("estado_LOPDP")` a `HasColumnName("Estado_LOPDP")`.
3. Esta corrección se incluye en la migración consolidada (sección siguiente).
4. En la migración, usar `RenameColumn` (disponible en EF Core para SQL Server):
   ```csharp
   migrationBuilder.RenameColumn(
       name: "estado_LOPDP",
       schema: "operativo",
       table: "Tbl_Direccion_Cliente",
       newName: "Estado_LOPDP");
   ```

### Consideraciones

- `RenameColumn` en SQL Server ejecuta internamente `sp_rename`, que es operación de metadatos y no mueve datos. Es instantáneo sin importar el volumen de filas.
- Verificar que ninguna vista SQL, stored procedure ni job del esquema `CI_DB/SDH.SQL.DB/` referencie la columna por su nombre en minúsculas antes de ejecutar en producción.

### Criterios de aceptación

- La columna en base de datos se llama `Estado_LOPDP` (verificar con `SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Tbl_Direccion_Cliente'`).
- `CustomerAddressConfiguration.cs` usa `HasColumnName("Estado_LOPDP")`.
- Ninguna vista ni procedimiento almacenado referencia `estado_LOPDP`.

---

## Migración Consolidada DT-03, DT-04, DT-05

Los tres items que requieren migración EF se consolidan en una sola migración para minimizar operaciones DDL en base de datos y simplificar el historial de migraciones.

### Nombre de la migración

```
DT_CorreccionesEsquema_R110
```

### Comando para generar la migración

Ejecutar desde `CI_MVC/contactabilidad_inteligente.mcv/contactabilidad_inteligente.mcv/`:

```bash
dotnet ef migrations add DT_CorreccionesEsquema_R110 \
  -p ../../../CI_API/SDH.infrastructure \
  -s . \
  --output-dir Persistence/Migrations
```

### Contenido esperado de la migración generada

EF Core generará automáticamente los cambios para DT-03 (MaxLength) y DT-05 (RenameColumn). El renombrado del índice DT-04 **no es detectado automáticamente** por EF (solo detecta que el nombre cambió, lo que puede traducirse a drop + create). Revisar el archivo generado y ajustar manualmente si EF no genera el drop/create correcto del índice.

Estructura esperada del método `Up`:
```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // DT-03: Ajuste de MaxLength (puede o no generar AlterColumn según versión de EF)
    migrationBuilder.AlterColumn<string>(
        name: "Identificacion_Cliente",
        schema: "operativo",
        table: "Tbl_Maest_Cliente",
        type: "nvarchar(20)",
        maxLength: 20,
        nullable: false,
        oldClrType: typeof(string),
        oldType: "nvarchar(32)",
        oldMaxLength: 32);

    // DT-04: Renombrado de índice
    migrationBuilder.DropIndex(
        name: "IX_Contacto_Cliente_IdCliente_EstaEliminado",
        schema: "operativo",
        table: "Tbl_Direccion_Cliente");

    migrationBuilder.CreateIndex(
        name: "IX_Direccion_Cliente_IdCliente_EstaEliminado",
        schema: "operativo",
        table: "Tbl_Direccion_Cliente",
        columns: new[] { "Id_Cliente", "Esta_Eliminado" });

    // DT-05: Renombrado de columna
    migrationBuilder.RenameColumn(
        name: "estado_LOPDP",
        schema: "operativo",
        table: "Tbl_Direccion_Cliente",
        newName: "Estado_LOPDP");
}
```

Estructura esperada del método `Down`:
```csharp
protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.AlterColumn<string>(
        name: "Identificacion_Cliente",
        schema: "operativo",
        table: "Tbl_Maest_Cliente",
        type: "nvarchar(32)",
        maxLength: 32,
        nullable: false,
        oldClrType: typeof(string),
        oldType: "nvarchar(20)",
        oldMaxLength: 20);

    migrationBuilder.DropIndex(
        name: "IX_Direccion_Cliente_IdCliente_EstaEliminado",
        schema: "operativo",
        table: "Tbl_Direccion_Cliente");

    migrationBuilder.CreateIndex(
        name: "IX_Contacto_Cliente_IdCliente_EstaEliminado",
        schema: "operativo",
        table: "Tbl_Direccion_Cliente",
        columns: new[] { "Id_Cliente", "Esta_Eliminado" });

    migrationBuilder.RenameColumn(
        name: "Estado_LOPDP",
        schema: "operativo",
        table: "Tbl_Direccion_Cliente",
        newName: "estado_LOPDP");
}
```

### Aplicar la migración

```bash
dotnet ef database update DT_CorreccionesEsquema_R110 \
  -p ../../../CI_API/SDH.infrastructure \
  -s .
```

---

## Orden de ejecución recomendado

```
DT-01 → DT-02 → DT-03 (código) → DT-04 (código) → DT-05 (código)
         → generar migración consolidada → revisar migración → aplicar
```

DT-01 y DT-02 son independientes entre sí y pueden implementarse en paralelo por personas distintas. Los cambios de código de DT-03, DT-04 y DT-05 deben completarse antes de generar la migración.

---

## Verificación global post-implementación

Tras completar todos los items, ejecutar:

```bash
# 1. Compilación limpia
dotnet build

# 2. Verificar que el modelo EF no registra diferencias pendientes
dotnet ef migrations add VerificacionPost_DT01-05 --dry-run \
  -p ../../../CI_API/SDH.infrastructure \
  -s .
# El resultado debe decir: "No changes detected"

# 3. Aplicar migración en ambiente de prueba
dotnet ef database update DT_CorreccionesEsquema_R110 \
  -p ../../../CI_API/SDH.infrastructure \
  -s .
```

Verificaciones SQL post-migración:
```sql
-- DT-03: Confirmar tamaño de columna
SELECT COLUMN_NAME, CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'operativo'
  AND TABLE_NAME = 'Tbl_Maest_Cliente'
  AND COLUMN_NAME = 'Identificacion_Cliente';
-- Esperado: 20

-- DT-04: Confirmar nombre de índice
SELECT name FROM sys.indexes
WHERE object_id = OBJECT_ID('operativo.Tbl_Direccion_Cliente')
  AND name LIKE 'IX_%IdCliente%';
-- Esperado: IX_Direccion_Cliente_IdCliente_EstaEliminado

-- DT-05: Confirmar nombre de columna
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'operativo'
  AND TABLE_NAME = 'Tbl_Direccion_Cliente'
  AND COLUMN_NAME = 'Estado_LOPDP';
-- Esperado: Estado_LOPDP
```

---

## Riesgos

| Riesgo | Probabilidad | Impacto | Mitigación |
|--------|-------------|---------|------------|
| `CustomerCommandService` tiene más llamadas a `.ClientId` o `.Create()` no identificadas | Baja | Bajo | El compilador las detecta como error; no hay riesgo silencioso |
| Alguna vista o SP referencia `estado_LOPDP` por nombre directo | Media | Medio | Buscar en `CI_DB/SDH.SQL.DB/` antes de ejecutar migración en producción |
| El renombrado del índice genera bloqueo en producción | Baja | Medio | Ejecutar en ventana de mantenimiento; considerar `sp_rename` directo si el volumen es alto |
| DT-03 falla porque existen datos de más de 20 caracteres | Muy baja | Alto | Verificar con `SELECT MAX(LEN(...))` antes de aplicar la migración |

---

## Dependencias con otros items del Release 1.1.0

- DT-02 (parametrizar Source) es prerequisito para el Módulo de Carga Masiva operativo: el campo `Source` debe recibir el identificador del proceso batch.
- DT-05 (renombrar `Estado_LOPDP`) es prerequisito para el Módulo LOPDP completo: el módulo escribe en esa columna.
- DT-01 no tiene dependencias externas dentro del release.
