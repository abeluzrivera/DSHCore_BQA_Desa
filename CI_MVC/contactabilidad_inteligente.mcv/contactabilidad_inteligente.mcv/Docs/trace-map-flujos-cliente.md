# Trace Map — Flujos de Cliente

> **Auditoría técnica** — Fecha: 2026-03-06  
> Ámbito: Dashboard de contactabilidad inteligente (ASP.NET Core Razor Pages + EF Core + JS)

---

## Índice

1. [Flujo 1 — Búsqueda de Clientes](#flujo-1--búsqueda-de-clientes)
   - 1a. Sugerencias en tiempo real (dropdown)
   - 1b. Carga del panel de detalle (sub-flujo)
2. [Flujo 2 — Aprobación de Contacto (Verificación)](#flujo-2--aprobación-de-contacto-verificación)
3. [Flujo 3 — Rechazo de Contacto (Marcar Error)](#flujo-3--rechazo-de-contacto-marcar-error)
4. [Flujo 4 — Carga Masiva (Botón "Cargar")](#flujo-4--carga-masiva-botón-cargar)
   - 4a. Tab "Cargar Archivo" → página BulkUpload
   - 4b. Tab "Pegar IDs" → carga directa en Dashboard
5. [Banderas de Auditoría Consolidadas](#banderas-de-auditoría-consolidadas)

---

## Flujo 1 — Búsqueda de Clientes

### 1a. Sugerencias en tiempo real

- **[`_DsClientSearch.cshtml` / `ds-search.js`] :: `input` event en `#ds-search-input`**
  - Debounce 300 ms; AbortController cancela petición anterior.
  - Guard: `q.trim().length < 3` → no dispara fetch.
  - **→ `GET /Dashboard?handler=SearchByName&q=…`**
    - **[`Dashboard.cshtml.cs`] :: `OnGetSearchByNameAsync(string q)`**
      - Guard duplicado: `q.Trim().Length < 3` → `[]` vacío. 🚩 **Redundancia** — validación ya aplicada en JS.
      - **→ [`ClienteCacheService`] :: `BuscarPorNombreAsync(q)`**
        - Crea scope con `IServiceScopeFactory` para resolver `IClienteRepository` (Scoped desde Singleton).
        - **→ [`ClienteRepository`] :: `BuscarLightAsync(nombre, top: 10)`**
          - SQL generado: `SELECT IdCliente, IdentificacionCliente, NombreCompleto, EstaVerificado … WHERE NombreCompleto LIKE '%nombre%' ORDER BY NombreCompleto OFFSET 0 ROWS FETCH NEXT 10 ROWS ONLY`
          - Proyección directa a `ClienteSearchResultDto` — sin `.Include()`. ✅ Diseño correcto.
          - Devuelve: `IReadOnlyList<ClienteSearchResultDto>`
        - Siembra `IMemoryCache` clave `"cliente:{cedula}"` con TTL 15 min (solo si no existe).
        - Devuelve: `IReadOnlyList<ClienteSearchResultDto>`
      - Proyecta a `{ id, name, initials, status }`.
      - Devuelve: `JsonResult { success, data[] }`
  - JS renderiza dropdown con nombre, cédula y badge de estado.

---

### 1b. Carga del panel de detalle (sub-flujo al hacer clic en "Agregar")

- **[`ds-search.js`] :: `click` en ítem del dropdown**
  - **→ `GET /Dashboard?handler=ClientDetail&id=…`**
    - **[`Dashboard.cshtml.cs`] :: `OnGetClientDetailAsync(string id)`**
      - **→ [`ClienteRepository`] :: `ObtenerPorIdentificacionAsync(identificacion)`**
        - SQL con `.Include()` filtrado:
          ```sql
          -- EF Core genera un JOIN con WHERE filtrado en el lado del cliente:
          WHERE ct.EstaEliminado = 0
            AND ct.EstadoVerificacion IN ('PEND', 'VERIF')
          ORDER BY ct.TipoMedioContacto ASC, ct.EstadoVerificacion DESC
          ```
          🚩 **Logic Leak / Over-Fetching** — Ver [Bandera A](#bandera-a--include-filtrado-excluye-contactos-err_).
        - Devuelve: `Cliente` (con `Contactos` parcialmente cargados).
      - `MapClienteToViewModel(cliente)` proyecta a `ClientViewModel`.
      - Devuelve: `JsonResult { success, data: ClientViewModel }`
  - JS inyecta artículo en `#ds-client-list-body` y abre panel `.dash-detail`.

---

## Flujo 2 — Aprobación de Contacto (Verificación)

- **[`dashboard.js`] :: `click` en `.contact-verify-btn`**
  - Guard: si `icon.classList.contains('is-verified')` → no-op (no se puede desverificar desde UI).
  - **Optimistic UI** (antes del API call):
    - Añade `dash-contact-row--verified`; remueve `dash-contact-row--error` y error hint.
    - `_setIconVerified(icon, true)` → aplica `is-verified`.
    - `_syncClientBadge(clientId)` → recalcula badge en DOM usando `CONTACT_TYPE_TO_GROUP` (JS). 🚩 Ver [Bandera B](#bandera-b--sincronía-optimista-antes-del-api-en-verificación).
  - **→ `_apiVerify({ clientId, contactType, contactValue, verified: true })`**
    - `POST /Dashboard?handler=UpdateVerification`
      - **[`Dashboard.cshtml.cs`] :: `OnPostUpdateVerificationAsync()`**
        - Lee cuerpo raw `StreamReader` + `JsonSerializer.Deserialize<ContactVerificationRequest>`.
        - Guard: campos nulos → 400.
        - **→ [`ClienteRepository`] :: `ObtenerPorIdentificacionAsync(clientId)`**
          - Mismo Include filtrado (ERR_* ausentes). 🚩 Bandera A.
        - Busca contacto: `cliente.Contactos.FirstOrDefault(c => c.ValorContacto == value && !c.EstaEliminado)`.
        - **→ [`MedioContacto`] :: `Verificar(usuario, "VERIF")`** — método de dominio.
        - **→ [`Cliente`] :: `ActualizarEstadoVerificacion()`**
          - `activos = _contactos.Where(!EstaEliminado)` — solo ve PEND+VERIF (ERR_* invisibles).
          - Agrupa por `GetGrupoContacto(TipoMedioContacto)`.
          - `EstaVerificado = ∀ grupo, ∃ contacto c | estado == "VERIF"`.
        - **→ `IUnitOfWork.SaveChangesAsync()`** — persiste cambios.
        - Devuelve: `JsonResult { success, data: { clienteVerificado, estadoVerificacion, … } }`
    - JS: `clienteVerificado` solo se usa para el texto del toast. `_syncClientBadge` **NO** se vuelve a llamar tras éxito del API. 🚩 Ver [Bandera B](#bandera-b--sincronía-optimista-antes-del-api-en-verificación).
    - En caso de error HTTP: revierte icono y re-llama `_syncClientBadge`.

---

## Flujo 3 — Rechazo de Contacto (Marcar Error)

- **[`dashboard.js`] :: `click` en `.contact-reject-btn`**
  - `_toggleRejectionZone(row)` — renderiza zona inline con pills de error según `ERROR_CATALOG[contactType]`.
  - **Usuario hace clic en una pill de error:**
    - Cierra zona de rechazo.
    - **→ `_apiContactError({ clientId, contactType, contactValue, errorCode, errorLabel })`**
      - `POST /Dashboard?handler=UpdateContactError`
        - **[`Dashboard.cshtml.cs`] :: `OnPostUpdateContactErrorAsync()`**
          - Lee cuerpo raw + deserializa `ContactErrorUpdateRequest`.
          - Guard: campos nulos → 400.
          - **→ [`ClienteRepository`] :: `ObtenerPorIdentificacionAsync(clientId)`**
            - Mismo Include filtrado. 🚩 Bandera A.
          - Busca contacto igual que en Flujo 2.
          - **→ [`MedioContacto`] :: `MarcarComoError(usuario, errorCode, usuario)`** — método de dominio.
          - **→ [`Cliente`] :: `ActualizarEstadoVerificacion()`** — misma lógica con ERR_* invisibles.
          - **→ `IUnitOfWork.SaveChangesAsync()`**
          - Devuelve: `JsonResult { success, data: { errorCode, clienteVerificado, … } }`
      - **Post-API (a diferencia de Flujo 2):** actualiza DOM de fila, inyecta error hint.
      - `_syncClientBadge(row.dataset.clientId)` ← llamado **después** del API. ✅ Correcto.

---

## Flujo 4 — Carga Masiva (Botón "Cargar")

- **[`Dashboard.cshtml`] :: `click` en `#btn-open-lote-modal`**
  - `_bindLoteModal()` (en `dashboard.js`) abre overlay `#lote-modal-overlay`.
  - Modal con dos tabs: **"Cargar Archivo"** y **"Pegar IDs"**.

---

### 4a. Tab "Cargar Archivo" → página BulkUpload

- **[`Dashboard.cshtml` / `dashboard.js`] :: `click` en `#lote-modal-submit` con tab activa `lote-tab-archivo`**
  - No ejecuta ninguna acción del servidor; redirige al cliente mediante `window.location.href = '/BulkUpload'`.
  - **→ `GET /BulkUpload`**
    - **[`BulkUpload.cshtml.cs`] :: `OnGet()`** — renderiza la página (sin lógica de datos).

- **[`BulkUpload.cshtml` / `bulkupload.js`] :: `change` en `#fileInput` (o drop en `#dropzone`)**
  - `handleFileSelect()`:
    - Validación client-side: extensión solo `.xlsx/.xls/.csv`.
    - Validación client-side: tamaño ≤ 25 MB.
    - Si pasa: `showFilePreview()` → muestra nombre/tamaño, habilita `#uploadBtn`.

- **[`bulkupload.js`] :: `click` en `#uploadBtn`**
  - `startUpload()`:
    - Registra entrada en `localStorage` (`uploadHistory`, máx. 5 items). 🚩 Ver [Bandera E](#bandera-e--historial-de-carga-en-localstorage).
    - Avanza visualmente a Paso 2 (`changeStep(2)`).
    - `simulateUpload()` — anima barra de progreso en cliente durante ~3 s (falsa). 🚩 Ver [Bandera F](#bandera-f--progreso-simulado-no-real).
    - **→ `processFile()`**
      - Construye `FormData` con el archivo.
      - **→ `POST /BulkUpload?handler=Upload`**
        - **[`BulkUpload.cshtml.cs`] :: `OnPostUploadAsync(IFormFile file)`**
          - Validación server-side: extensión `.xlsx/.xls/.csv` (duplicada). 🚩 **Redundancia** con client-side.
          - Validación server-side: tamaño ≤ 25 MB (duplicada). ✅ Esto sí es correcto.
          - **⚠️ Procesamiento simulado:** `await Task.Delay(3000)` + resultados hardcodeados. 🚩 Ver [Bandera G](#bandera-g--procesamiento-de-archivo-no-implementado).
          - No invoca ningún Service ni Repository.
          - Devuelve: `JsonResult { success, fileName, totalRecords, successCount, errorCount, errors[] }`
      - Si `result.success`: `showReport(result)` → rellena tabla de errores; `changeStep(3)`.
      - Si error HTTP/JS: `resetUpload()` → vuelve a Paso 1.

- **[`BulkUpload.cshtml.cs`] :: `OnGetDownloadTemplate()`** (flujo auxiliar)
  - Busca `wwwroot/Template/Plantilla_Contactabilidad_Cliente.xlsx` en 3 rutas posibles.
  - Devuelve `File(bytes, contentType, fileName)` para descarga directa.

- **[`BulkUpload.cshtml.cs`] :: `OnGetDownloadErrorLog(string fileName, string errors)`** (flujo auxiliar)
  - Genera y devuelve un archivo de log de errores para descarga.

---

### 4b. Tab "Pegar IDs" → carga directa en Dashboard

- **[`Dashboard.cshtml` / `dashboard.js`] :: `input` en `#lote-ids-textarea`**
  - `_parseIds()`: split por `[\n,;\s]+`, deduplica, valida formato de ID.
  - `_updateBanner()`: muestra banner `N IDs válidos • M duplicados eliminados · K inválidos ignorados`.

- **[`dashboard.js`] :: `click` en `#lote-modal-submit` con tab activa `lote-tab-ids`**
  - `_parseIds(textarea.value).valid` → array de IDs válidos.
  - Guard: si `valid.length === 0` → foco en textarea, no navega.
  - Redirige: `window.location.href = '/dashboard?ids=id1,id2,...'`.
  - **→ `GET /Dashboard?ids=…`**
    - **[`Dashboard.cshtml.cs`] :: `OnGetAsync()`**
      - `FilterIds` recibe los IDs via `[BindProperty(SupportsGet=true)]`.
      - **→ `LoadClientsAsync()`**
        - Por cada ID en `FilterIds.Split(',')`:
          - **→ [`ClienteRepository`] :: `ObtenerPorIdentificacionAsync(id)`**
            - Include filtrado (`PEND` + `VERIF` únicamente). 🚩 Bandera A.
        - Itera N consultas secuenciales (1 query por ID). 🚩 Ver [Bandera H](#bandera-h--n-queries-secuenciales-en-loadclientsasync).
        - Llena `Clients` con `MapClienteToViewModel(cliente)`.
      - Renderiza la página completa con los clientes pre-cargados en el listado.

  - **Adicionalmente (si la página ya está en memoria / JS activo):**
    - `_apiSearchClientsByIds(ids[])` → `POST /Dashboard?handler=SearchClientsByIds`
      - **[`Dashboard.cshtml.cs`] :: `OnPostSearchClientsByIdsAsync()`**
        - Itera IDs, llama `ObtenerPorIdentificacionAsync` por cada uno. 🚩 Bandera H (misma).
        - Devuelve: `JsonResult { success, data: ClientViewModel[] }`
      - JS inyecta artículos en `#ds-client-list-body` sin recargar página.

---

## Banderas de Auditoría Consolidadas

### Bandera A — `.Include()` filtrado excluye contactos `ERR_*`

| Atributo | Detalle |
|---|---|
| **Tipo** | Logic Leak + Over-Fetching (por defecto) |
| **Archivo** | [`Infrastructure/Persistence/Repositories/ClienteRepository.cs`](../Infrastructure/Persistence/Repositories/ClienteRepository.cs) |
| **Métodos** | `ObtenerPorIdentificacionAsync`, `ObtenerPorIdAsync` |
| **Impacto** | El filtro `EstadoVerificacion IN ('PEND', 'VERIF')` dentro del `.Include()` silencia todos los contactos en estado `ERR_*` y sus derivados. La colección `_contactos` que recibe `ActualizarEstadoVerificacion()` es **incompleta**. Si todos los contactos de un grupo fueron rechazados (todos `ERR_*`), el grupo desaparece de la vista del dominio y no bloquea la verificación del cliente — cliente marcado `EstaVerificado = true` incorrectamente. |
| **Corrección sugerida** | Remover el filtro de `EstadoVerificacion` del `.Include()`. Si se necesita orden específico, conservar solo `OrderBy`. El filtrado por estado debe decidirse en la capa de dominio/servicio, no en el Repository. |

---

### Bandera B — Sincronía optimista antes del API en Verificación

| Atributo | Detalle |
|---|---|
| **Tipo** | Redundancia / Inconsistencia de flujo |
| **Archivo** | [`wwwroot/js/dashboard.js`](../wwwroot/js/dashboard.js) |
| **Función** | `_apiVerify` vs `_apiContactError` |
| **Impacto** | En Flujo 2 (aprobación), `_syncClientBadge` se llama **antes** del POST y el resultado del servidor (`clienteVerificado`) solo alimenta el texto del toast, no corrige el badge. En Flujo 3 (rechazo), `_syncClientBadge` se llama **después** del POST. Esta asimetría implica que si el servidor y el cliente calculan `EstaVerificado` de forma diferente (ej. por la Bandera A), el badge en pantalla quedará desincronizado con la BD permanentemente hasta que se recargue la página. |
| **Corrección sugerida** | En `_apiVerify`, mover `_syncClientBadge` al bloque de éxito del `try` (después del POST). Si se desea mantener el optimistic UI, al menos llamar `_syncClientBadge` de nuevo con el valor autoritativo del servidor. |

---

### Bandera C — Divergencia potencial entre mapas de grupo (JS vs C#)

| Atributo | Detalle |
|---|---|
| **Tipo** | Redundancia / Riesgo de drift |
| **Archivos** | [`wwwroot/js/dashboard.js`](../wwwroot/js/dashboard.js) `CONTACT_TYPE_TO_GROUP` (línea 82) y [`Domain/Entities/Operativo/Cliente.cs`](../Domain/Entities/Operativo/Cliente.cs) `GetGrupoContacto()` (línea 88) |
| **Estado actual** | **Sincronizados** — ambos mapean `{CEL,TEL_C,WAPP}→phone`, `EMAIL→email`, `{DIR_D,DIR_T}→address`, `{REF_P,REF_C}→reference`, `EMERG→emergency`, `LINK→social`. |
| **Riesgo** | Son dos fuentes de verdad independientes. Al agregar un nuevo tipo de contacto al catálogo, es fácil actualizar solo una de las dos. No existe validación automática entre ellas. |
| **Corrección sugerida** | Exponer el mapa desde un endpoint de configuración (ej. `GET /Dashboard?handler=ContactGroupMap`) o generarlo desde el servidor en el Razor layout `/Pages/_ViewImports.cshtml`. |

---

### Bandera D — Método `BuscarPorNombreAsync` (heavy) posiblemente huérfano

| Atributo | Detalle |
|---|---|
| **Tipo** | Redundancia |
| **Archivo** | [`Infrastructure/Persistence/Repositories/ClienteRepository.cs`](../Infrastructure/Persistence/Repositories/ClienteRepository.cs) |
| **Método** | `BuscarPorNombreAsync(string, int, CancellationToken)` — carga `.Include(Contactos)` completo. |
| **Situación** | El flujo de búsqueda usa `BuscarLightAsync` (proyección, sin Include). El método pesado `BuscarPorNombreAsync` está declarado en la interfaz `IClienteRepository` pero no aparece llamado por ningún Service en el flujo activo de búsqueda. |
| **Riesgo** | Mantiene una ruta de acceso sin usar que trae datos de contactos completos innecesariamente si llegara a invocarse. Confunde la interfaz del repositorio. |
| **Corrección sugerida** | Verificar si hay consumidores fuera del dashboard (ej. `CatalogoService`, jobs de batch). Si no los hay, deprecated y eliminar de la interfaz. |

---

### Bandera E — Historial de carga en `localStorage`

| Atributo | Detalle |
|---|---|
| **Tipo** | Diseño / Riesgo de privacidad |
| **Archivo** | [`wwwroot/js/bulkupload.js`](../wwwroot/js/bulkupload.js) — `addToUploadHistory()` |
| **Impacto** | Los nombres de archivos cargados se guardan en `localStorage` del navegador (máx. 5). En equipos compartidos o sesiones de diferentes usuarios en la misma máquina, el historial de un usuario queda visible para otro. No está asociado a la sesión del servidor. |
| **Corrección sugerida** | Usar `sessionStorage` en lugar de `localStorage`, o bien guardar el historial server-side ligado al usuario autenticado (ej. `TempData` o tabla de auditoría). |

---

### Bandera F — Progreso simulado no real

| Atributo | Detalle |
|---|---|
| **Tipo** | Inconsistencia UX / Deuda técnica |
| **Archivo** | [`wwwroot/js/bulkupload.js`](../wwwroot/js/bulkupload.js) — `simulateUpload()` |
| **Impacto** | La barra de progreso del Paso 2 es una animación aleatoria (`Math.random() * 15`) completamente desconectada del procesamiento real. El servidor aplica `await Task.Delay(3000)` como placeholder. El usuario ve un progreso ficticio. Si el archivo real tarda más o menos, la UX engañará al usuario. |
| **Corrección sugerida** | Implementar progreso real via `SignalR`, SSE (Server-Sent Events), o polling `GET /BulkUpload?handler=UploadStatus&jobId=…` con un identificador de tarea asíncrona. |

---

### Bandera G — Procesamiento de archivo no implementado

| Atributo | Detalle |
|---|---|
| **Tipo** | Deuda técnica crítica |
| **Archivo** | [`Pages/BulkUpload.cshtml.cs`](../Pages/BulkUpload.cshtml.cs) — `OnPostUploadAsync()` |
| **Impacto** | El handler actual solo guarda el archivo en memoria (como `IFormFile`), duerme 3 segundos y devuelve **resultados hardcodeados** (`totalRecords=500`, `successCount=482`, 18 errores fijos). No invoca ningún Service, no lee el Excel/CSV, no persiste nada en base de datos. Toda la pantalla de "reporte de resultados" es ficticia. |
| **Corrección sugerida** | Implementar un `IClienteImportService` que: (1) parsee el Excel/CSV con EPPlus o CsvHelper, (2) valide cada fila, (3) persista via `IClienteRepository`, (4) retorne un `ImportResultDto` real con filas procesadas y errores por fila. |

---

### Bandera H — N queries secuenciales al buscar por lista de IDs

| Atributo | Detalle |
|---|---|
| **Tipo** | Over-Fetching / Rendimiento |
| **Archivos** | [`Pages/Dashboard.cshtml.cs`](../Pages/Dashboard.cshtml.cs) — `LoadClientsAsync()` y `OnPostSearchClientsByIdsAsync()` |
| **Impacto** | Ambos métodos iteran la lista de IDs y ejecutan `ObtenerPorIdentificacionAsync(id)` **uno por uno** en un `foreach`. Para un lote de 50 IDs se generan 50 roundtrips a la base de datos. |
| **Corrección sugerida** | Añadir `Task<IEnumerable<Cliente>> ObtenerPorIdentificacionesAsync(IEnumerable<string> ids)` al repositorio, generando una sola query `WHERE IdentificacionCliente IN (…)` con EF Core. |
