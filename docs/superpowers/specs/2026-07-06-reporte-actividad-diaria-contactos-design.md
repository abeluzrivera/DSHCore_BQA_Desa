# Diseño: Reporte Diario de Actividad de Contactos y Direcciones

**Fecha:** 2026-07-06  
**Autor:** Pedro Abel  
**Audiencia:** Alta gerencia via Power BI  
**Estado:** Aprobado

---

## Contexto

El sistema SDH registra dos tipos de actividad sobre los datos de contactabilidad de clientes:

1. **Confirmaciones** — un asesor verifica que un contacto o dirección es válido (`Fecha_Verificacion`, `Usuario_Verificador`)
2. **Adiciones** — un asesor agrega un nuevo contacto o dirección al sistema (`Fecha_Creacion`, `Usuario_Creacion`)

Este reporte mide la usabilidad y el valor generado por el sistema, presentando a gerencia cuántos registros se confirman y agregan cada día, desglosados por tipo y asesor.

### Restricción de carga inicial

El día de migración inicial cargó todos los registros históricos con la misma `Fecha_Creacion`. Ese día se excluye de la serie de tiempo del reporte para no distorsionar tendencias. Se persiste por separado como contexto de línea base.

---

## Fuentes de datos

| Tabla | Campos clave de confirmación | Campos clave de adición |
|---|---|---|
| `operativo.Tbl_Contacto_Cliente` | `Fecha_Verificacion`, `Usuario_Verificador` | `Fecha_Creacion`, `Usuario_Creacion` |
| `operativo.Tbl_Direccion_Cliente` | `Fecha_Verificacion`, `Usuario_Verificador` | `Fecha_Creacion`, `Usuario_Creacion` |

**Tipos de contacto conocidos:**
- 101 = Celular
- 102 = Correo
- 103 = Telefono Convencional

**Tipos de dirección conocidos:**
- 105 = Domicilio
- 106 = Trabajo

---

## Arquitectura

### Schema nuevo: `reportes`

Aislado de los schemas operativos. Toda la capa de reportería reside aquí.

```
reportes/
  Tbl_Baseline_Carga_Inicial    -- contexto de migración (registro único)
  Tbl_Det_Actividad_Diaria      -- snapshot de detalle diario (drill-down)
  Tbl_Sum_Actividad_Diaria      -- resumen pre-agregado (consumo Power BI)
  Tbl_Log_Jobs                  -- auditoría de ejecuciones de jobs
```

---

## Tablas

### `reportes.Tbl_Baseline_Carga_Inicial`

Registro único. Captura el estado del sistema el día de migración.

| Columna | Tipo | Nulable | Descripción |
|---|---|---|---|
| `Id_Baseline` | int IDENTITY PK | No | |
| `Fecha_Migracion` | date | No | Fecha detectada de carga inicial |
| `Total_Contactos` | int | No | Total de contactos migrados |
| `Total_Direcciones` | int | No | Total de direcciones migradas |
| `Contactos_Ya_Verificados` | int | No | Contactos que venían pre-verificados |
| `Direcciones_Ya_Verificadas` | int | No | Direcciones que venían pre-verificadas |
| `Fecha_Procesamiento` | datetime2(3) | No | Timestamp de ejecución del job |

---

### `reportes.Tbl_Det_Actividad_Diaria`

Una fila por cada contacto o dirección que fue confirmado o agregado en el día. Base para drill-down.

| Columna | Tipo | Nulable | Descripción |
|---|---|---|---|
| `Id_Detalle` | bigint IDENTITY PK | No | |
| `Fecha_Reporte` | date | No | Día que se reporta |
| `Tipo_Entidad` | nvarchar(15) | No | `'CONTACTO'` o `'DIRECCION'` |
| `Id_Entidad` | int | No | ID del registro original |
| `Id_Cliente` | bigint | No | FK cliente |
| `Tipo_Accion` | nvarchar(15) | No | `'CONFIRMADO'` o `'AGREGADO'` |
| `Id_Tipo_Detalle` | int | No | Id del tipo de contacto o dirección |
| `Texto_Tipo` | nvarchar(50) | No | Celular / Correo / Convencional / Domicilio / Trabajo |
| `Valor` | nvarchar(500) | Si | El valor del contacto o la dirección completa |
| `Usuario_Responsable` | nvarchar(100) | No | Asesor que confirmó o agregó |
| `Source` | nvarchar(50) | Si | Fuente del dato original |
| `Fecha_Procesamiento` | datetime2(3) | No | Timestamp de ejecución del job |

**Índices:**
- `IX_Det_FechaReporte` en `Fecha_Reporte`
- `IX_Det_UsuarioFecha` en `(Usuario_Responsable, Fecha_Reporte)`

---

### `reportes.Tbl_Sum_Actividad_Diaria`

Pre-agregada. Una fila por combinación de fecha + entidad + acción + tipo + usuario. Consumo directo de Power BI.

| Columna | Tipo | Nulable | Descripción |
|---|---|---|---|
| `Id_Resumen` | int IDENTITY PK | No | |
| `Fecha_Reporte` | date | No | El día |
| `Tipo_Entidad` | nvarchar(15) | No | `'CONTACTO'` o `'DIRECCION'` |
| `Tipo_Accion` | nvarchar(15) | No | `'CONFIRMADO'` o `'AGREGADO'` |
| `Texto_Tipo` | nvarchar(50) | No | Celular / Correo / Convencional / Domicilio / Trabajo |
| `Usuario_Responsable` | nvarchar(100) | No | Asesor |
| `Total_Registros` | int | No | Conteo del día para esa combinación |
| `Fecha_Procesamiento` | datetime2(3) | No | Timestamp de ejecución del job |

**Índices:**
- `IX_Sum_FechaReporte` en `Fecha_Reporte`
- `UQ_Sum_Combinacion` único en `(Fecha_Reporte, Tipo_Entidad, Tipo_Accion, Texto_Tipo, Usuario_Responsable)`

---

### `reportes.Tbl_Log_Jobs`

Auditoría de cada ejecución de job.

| Columna | Tipo | Nulable | Descripción |
|---|---|---|---|
| `Id_Log` | int IDENTITY PK | No | |
| `Nombre_Job` | nvarchar(100) | No | Nombre del job |
| `Fecha_Reporte` | date | Si | Día procesado (null para baseline) |
| `Estado` | nvarchar(20) | No | `'EXITOSO'` o `'ERROR'` |
| `Registros_Detalle` | int | Si | Filas insertadas en detalle |
| `Registros_Resumen` | int | Si | Filas insertadas en resumen |
| `Mensaje_Error` | nvarchar(max) | Si | Detalle del error si aplica |
| `Fecha_Ejecucion` | datetime2(3) | No | Timestamp de ejecución |

---

## Jobs SQL Agent

### `JOB_Baseline_Carga_Inicial`

- **Tipo:** Ejecución única, manual, post-creación de tablas
- **Idempotente:** Verifica existencia en `Tbl_Baseline_Carga_Inicial` antes de insertar

**Lógica:**
1. Detectar fecha de migración: `SELECT TOP 1 CAST(Fecha_Creacion AS date) FROM operativo.Tbl_Contacto_Cliente GROUP BY CAST(Fecha_Creacion AS date) ORDER BY COUNT(*) DESC`
2. Si ya existe registro en `Tbl_Baseline_Carga_Inicial`, salir sin insertar
3. Contar totales del día de migración en contactos y direcciones
4. Insertar en `Tbl_Baseline_Carga_Inicial`
5. Registrar en `Tbl_Log_Jobs` con `Nombre_Job = 'JOB_Baseline_Carga_Inicial'`

---

### `JOB_Reporte_Actividad_Diaria`

- **Tipo:** Recurrente, cada día a las 01:00 AM
- **Idempotente:** Elimina registros del día antes de re-insertar

**Lógica en orden:**
1. `SET @FechaReporte = CAST(DATEADD(day, -1, GETDATE()) AS date)`
2. Leer `@FechaMigracion` desde `Tbl_Baseline_Carga_Inicial`
3. Si `@FechaReporte = @FechaMigracion`, salir (no procesar día de migración)
4. Eliminar de `Tbl_Det_Actividad_Diaria` donde `Fecha_Reporte = @FechaReporte`
5. Eliminar de `Tbl_Sum_Actividad_Diaria` donde `Fecha_Reporte = @FechaReporte`
6. Insertar en `Tbl_Det_Actividad_Diaria`:
   - Contactos confirmados: `CAST(Fecha_Verificacion AS date) = @FechaReporte AND Esta_Eliminado = 0`
   - Contactos agregados: `CAST(Fecha_Creacion AS date) = @FechaReporte AND Esta_Eliminado = 0`
   - Direcciones confirmadas: `CAST(Fecha_Verificacion AS date) = @FechaReporte AND Esta_Eliminado = 0`
   - Direcciones agregadas: `CAST(Fecha_Creacion AS date) = @FechaReporte AND Esta_Eliminado = 0`
7. Agregar desde detalle e insertar en `Tbl_Sum_Actividad_Diaria`
8. Registrar en `Tbl_Log_Jobs` con conteos y estado

**Filtros de exclusión aplicados en todos los inserts:**
- `Esta_Eliminado = 0`
- `Fecha_Creacion` o `Fecha_Verificacion` distinta a `@FechaMigracion`

---

## Flujo de datos

```
operativo.Tbl_Contacto_Cliente  ─┐
                                  ├─► JOB nocturno ─► Tbl_Det_Actividad_Diaria ─► Power BI (drill-down)
operativo.Tbl_Direccion_Cliente ─┘         │
                                            └─► Tbl_Sum_Actividad_Diaria ──────► Power BI (KPIs gerencia)

JOB_Baseline (una vez) ─────────────────────► Tbl_Baseline_Carga_Inicial ──────► Power BI (línea base)

Todos los jobs ─────────────────────────────► Tbl_Log_Jobs (auditoría)
```

---

## Consideraciones futuras

- Cuando se agregue la tabla de empleados, `Usuario_Responsable` se podrá enriquecer con nombre completo, área y cargo mediante un JOIN adicional en el job.
- El schema `reportes` es extensible para otros reportes del sistema sin afectar los schemas operativos.
