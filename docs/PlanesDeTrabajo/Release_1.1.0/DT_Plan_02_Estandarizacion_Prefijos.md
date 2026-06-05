# Plan de Trabajo — Deuda Técnica Item 13
## Estandarización del Nombre de la Plataforma (SDH como nombre definitivo)
### Release 1.1.0 — Paquete E

**Fecha:** 05 de junio de 2026  
**Estado:** Listo para implementar  
**Responsable:** Por asignar  
**Estimación:** 1-2 días

---

## Decisión tomada

**El nombre definitivo de la plataforma es Smart Data Hub (SDH).**

Consecuencias directas:
- Los artefactos de código (`SDH.Domain`, `SDH.Application`, `SDH.infrastructure`) conservan su prefijo. No se requiere ningún renombrado de proyectos, carpetas ni namespaces.
- El ítem DT-06 (unificación de prefijo SDH → DSH) queda cancelado. El código ya usa el prefijo correcto.
- El ítem DT-13 (antes DT-12) se reduce a un trabajo de actualización de documentación y textos de interfaz: garantizar que la plataforma sea referida como "Smart Data Hub (SDH)" de forma consistente, eliminando cualquier aparición de "DSH" que aluda a esta plataforma.

---

## Objetivo

Establecer "Smart Data Hub (SDH)" como el nombre oficial y uniforme de la plataforma en toda la documentación, textos de interfaz y comunicaciones internas. Eliminar sistemáticamente las referencias a "DSH" que aludan a esta plataforma.

---

## Inventario de referencias a corregir

### Documentación técnica en el repositorio

Localizar todas las referencias a "DSH" que se refieren a la plataforma (no al banco u otros sistemas):

```powershell
# Buscar en documentación Markdown
Select-String -Path "D:\sources\DSHCore_BQA_Desa\docs\**\*.md" -Pattern "\bDSH\b" -Recurse |
  Select-Object Path, LineNumber, Line

# Buscar en CLAUDE.md
Select-String -Path "D:\sources\DSHCore_BQA_Desa\CLAUDE.md" -Pattern "\bDSH\b"
```

Archivos identificados con referencias a revisar:

| Archivo | Tipo de referencia |
|---------|-------------------|
| `docs/Archi/release_PGD.md` | El encabezado de plataforma dice "SDH" pero el texto de SNIPER dice "monolito DSH" |
| `docs/Tecnica/Manual_implementacion_IIS.md` | Verificar si menciona "DSH" como nombre de la plataforma |
| `docs/Archi/DSH_Arq_emp.archimate` | El nombre del archivo usa "DSH"; evaluar si renombrar el archivo |
| `CLAUDE.md` | Verificar referencias textuales a DSH en la descripción del proyecto |

### Textos visibles en la interfaz de usuario

Buscar en las vistas Razor y PageModels:

```powershell
# Títulos, labels, mensajes que digan "DSH"
Select-String -Path "D:\sources\DSHCore_BQA_Desa\CI_MVC\**\*.cshtml" -Pattern "\bDSH\b" -Recurse |
  Select-Object Path, LineNumber, Line

Select-String -Path "D:\sources\DSHCore_BQA_Desa\CI_MVC\**\*.cs" -Pattern '"DSH' -Recurse |
  Select-Object Path, LineNumber, Line
```

### Comentarios en código

Los comentarios en archivos `.cs` que mencionen "DSH" como nombre de la plataforma deben actualizarse. Los que mencionen "DSH" como nombre de otro sistema (Core bancario, etc.) se mantienen.

```powershell
Select-String -Path "D:\sources\DSHCore_BQA_Desa\CI_API\**\*.cs" -Pattern "//.*\bDSH\b" -Recurse |
  Select-Object Path, LineNumber, Line
```

---

## Pasos de implementación

### Paso 1 — Ejecutar el inventario completo

Ejecutar los comandos de búsqueda de la sección anterior y registrar todos los archivos afectados. No modificar nada hasta tener el inventario completo.

### Paso 2 — Actualizar documentación técnica

1. `docs/Archi/release_PGD.md`: la sección de SNIPER dice "Vive dentro del monolito DSH". Cambiar a "Vive dentro del monolito SDH".
2. `docs/Tecnica/Manual_implementacion_IIS.md`: revisar el documento completo. Reemplazar "DSH" por "SDH" donde se refiera a la plataforma. No alterar referencias a rutas de sistema operativo o nombres de grupo AD que digan "DSH" hasta que DT-07 (renombrado de grupos) esté completo.
3. `CLAUDE.md`: verificar la sección de descripción del proyecto. El archivo actualmente describe la plataforma; actualizar si dice "DSH".

### Paso 3 — Evaluar el renombrado del archivo de arquitectura

El archivo `docs/Archi/DSH_Arq_emp.archimate` tiene "DSH" en su nombre. Evaluar con el equipo si renombrar el archivo a `SDH_Arq_emp.archimate`. Si se decide renombrarlo:

```powershell
# Verificar que no haya referencias al nombre del archivo en otros documentos
Select-String -Path "D:\sources\DSHCore_BQA_Desa\**\*.md" -Pattern "DSH_Arq_emp" -Recurse

# Si no hay referencias externas, renombrar
Rename-Item "D:\sources\DSHCore_BQA_Desa\docs\Archi\DSH_Arq_emp.archimate" `
            "D:\sources\DSHCore_BQA_Desa\docs\Archi\SDH_Arq_emp.archimate"
# Ídem para el .bak
Rename-Item "D:\sources\DSHCore_BQA_Desa\docs\Archi\DSH_Arq_emp.archimate.bak" `
            "D:\sources\DSHCore_BQA_Desa\docs\Archi\SDH_Arq_emp.archimate.bak"
```

### Paso 4 — Actualizar textos de interfaz de usuario

Con el resultado del inventario de vistas Razor, actualizar cada ocurrencia de "DSH" en títulos de página, etiquetas, mensajes de error o cualquier texto visible al usuario.

Ejemplos probables a buscar:
- Títulos HTML: `<title>DSH — ...`
- Texto en navbar o sidebar: `<span>DSH</span>`
- Mensajes: `"Bienvenido a DSH"`

### Paso 5 — Actualizar comunicaciones internas (fuera del repositorio)

Esta tarea es organizacional, no técnica:
1. Notificar al área de comunicación interna del banco que el nombre oficial de la plataforma es SDH.
2. Solicitar que cualquier material de capacitación, correos o documentos internos que digan "DSH" para referirse a esta plataforma sean actualizados.
3. Verificar si hay tickets Jira, páginas Confluence o canales de Slack con el nombre "DSH" que deban actualizarse.

### Paso 6 — Actualizar documentación de usuario

Revisar el manual de usuario y cualquier guía operativa:
1. `docs/Tecnica/Manual_implementacion_IIS.md`: actualizar todas las referencias de nombre.
2. Si existe un manual de operador (puede estar fuera del repositorio), coordinarlo con el área responsable.
3. Verificar que las capturas de pantalla en la documentación no muestren "DSH" en la interfaz (si hay capturas previas a este cambio).

---

## Criterios de aceptación

- `grep -r "\bDSH\b" docs/` devuelve cero resultados que se refieran a la plataforma SDH (pueden existir referencias a otros sistemas del banco con ese nombre).
- Ninguna vista `.cshtml` ni PageModel muestra "DSH" como texto al usuario final.
- `docs/Archi/release_PGD.md` usa "SDH" de forma consistente para referirse a la plataforma.
- El manual de implementación y documentación de usuario no mezcla los dos nombres.

---

## Riesgos

| Riesgo | Probabilidad | Impacto | Mitigación |
|--------|-------------|---------|------------|
| Alguna referencia a "DSH" corresponde al banco u otro sistema, no a la plataforma, y se modifica incorrectamente | Media | Bajo | Revisar el contexto de cada ocurrencia antes de modificarla; no hacer reemplazo global ciego |
| El archivo `.archimate` tiene el nombre "DSH" referenciado en herramientas externas (Archi, Confluence) | Baja | Medio | Verificar antes de renombrar el archivo; si hay referencias externas, mantener el nombre del archivo y solo actualizar el contenido interno |

---

## Nota sobre DT-06

El ítem DT-06 (unificación de prefijo de artefactos SDH → DSH) está cancelado en este release. Los artefactos de código conservan el prefijo `SDH`. Si en el futuro el banco decide cambiar la denominación oficial a DSH, ese trabajo se planifica como un ítem independiente con el alcance completo descrito en la versión anterior de este plan (disponible en el historial de git).
