# 🏢 PROTOCOLO: FIGMA-TO-RAZOR — SMART HUB BM (v4.0)
**Binding:** Design System Smart Hub Enterprise v2.0  
**Proyecto:** BM — .NET MVC + Razor + Bootstrap (legacy) + Tailwind (`tw-*`) + DS Custom (`ds-*`)  
**Archivo de referencia:** `Design_System_BM.md`

> **Dependencia:** Este archivo extiende `protocolo-figma-to-razor.md` (protocolo genérico).
> Ambos archivos deben estar cargados en contexto para una ejecución correcta.

---

## 🗂️ CONTEXTO DEL STACK

### Cascada CSS (orden de especificidad)
```
Bootstrap (base) → Tailwind (tw-*) → Tokens (variables.css) → DS (ds/ds.css) → Overrides → Page CSS
```

### Fuentes de verdad
| Recurso | Archivo | Contiene |
|---------|---------|----------|
| **Tokens** | `wwwroot/css/variables.css` | Custom properties en `:root` — colores, espaciado, tipografía, sombras, radius, z-index, transiciones |
| **Componentes DS** | `wwwroot/css/ds/components/_*.css` | 8 archivos BEM: buttons, cards, badges, tables, forms, feedback, layout, search |
| **Tailwind config** | `tailwind.config.js` | Prefijo `tw-`, sin preflight, colores espejo de tokens DS |
| **Partials Razor** | `Pages/Shared/Components/_Ds*.cshtml` | PageHeader, Badge, Card, Modal, ClientSearch, ClientList |
| **Módulos JS** | `wwwroot/js/ds/ds-*.js` | Namespace `window.DS` — api, notify, modal, form, search, events, utils, state |

---

## 🎯 REGLAS DE PRIORIDAD BM

### ¿Qué usar y cuándo?

| Necesidad | Usar | NO usar |
|-----------|------|---------|
| Botón | `.ds-btn .ds-btn--{variant}` | Bootstrap `.btn`, Tailwind `tw-bg-*` |
| Card | `.ds-card` + hijos (`__header`, `__body`, `__footer`) | Div genérico con estilos inline |
| Badge / Pill | `.ds-badge .ds-badge--{variant}` | `<span>` con colores hardcoded |
| Tabla | `.ds-table-wrapper` + `.ds-table` | Bootstrap `table-*` |
| Formulario | `.ds-form-group`, `.ds-input`, `.ds-select`, `.ds-textarea` | Bootstrap `form-control` |
| Alert | `.ds-alert .ds-alert--{variant}` | Bootstrap alert |
| Modal | `.ds-modal-backdrop` + `.ds-modal` + `DS.modal.open()` | Bootstrap modal |
| Toast / Notificación | `DS.notify.success()` / `.error()` / `.warning()` / `.info()` | `alert()`, `console.log` |
| Confirmación | `DS.notify.confirm()` | `confirm()` nativo |
| HTTP requests | `DS.api.get()` / `.post()` / `.put()` / `.delete()` | `fetch()` directo |
| Validación forms | `DS.form.validate()` | Validación manual |
| Iconos principales | `<span class="material-symbols-outlined">icon_name</span>` | Font Awesome, otros icon fonts |
| Iconos SVG custom | `<img src="~/icons/nombre.svg" class="ds-icon" aria-hidden="true" alt="" />` | SVG inline, `<i>` tags |
| Layout de comp. aislado | Tailwind `tw-flex`, `tw-p-4`, etc. | Clases DS donde no existe componente |
| Colores, sombras, spacing | Custom properties `var(--color-*)`, `var(--spacing-*)`, `var(--shadow-*)` | HEX/RGBA directo |

---

## 🔄 MAPEO DE TOKENS BM

### Colores Figma → Tokens

| Color Figma (HEX/RGBA) | Token BM | Uso semántico |
|-------------------------|----------|---------------|
| `#446942` | `var(--color-primary)` | Verde marca principal |
| `#7F9787` | `var(--color-primary-light)` | Hover, acentos suaves |
| `#295136` | `var(--color-primary-dark)` | Texto énfasis, bordes fuertes |
| `rgba(68,105,66, 0.05)` | `var(--color-primary-50)` | Fondo ultra-sutil |
| `rgba(68,105,66, 0.10)` | `var(--color-primary-10)` | Fondo sutil |
| `#f5f7f5` | `var(--color-bg-page)` | Fondo de página |
| `#ffffff` | `var(--color-bg-surface)` | Cards, modales |
| `#eef2ee` | `var(--color-bg-subtle)` | Fondos sutiles |
| `#ebf2ee` | `var(--color-bg-selected)` | Fila activa |
| `#DBE4DD` | `var(--color-bg-light)` | Fondo decorativo |
| `#171b17` | `var(--color-bg-dark)` | Fondo oscuro |
| `#F3F4F6` | `var(--color-neutral-100)` | Gris neutro |
| `#28a745` | `var(--color-success)` | Verde éxito |
| `#dc3545` | `var(--color-danger)` | Rojo error |
| `#ffc107` | `var(--color-warning)` | Amarillo advertencia |
| `#0dcaf0` | `var(--color-info)` | Cyan información |
| `#CB801B` | `var(--color-alert)` | Ocre/ámbar alerta |
| `#dc2626` | `var(--color-rejection)` | Rechazo |

**Textos:**

| Figma | Token |
|-------|-------|
| `#295136` | `var(--color-text-primary)` / clase `.text-primary` |
| `rgba(41,81,54, 0.70)` | `var(--color-text-secondary)` / clase `.text-secondary` |
| `rgba(41,81,54, 0.50)` | `var(--color-text-muted)` / clase `.text-muted` |
| `rgba(41,81,54, 0.30)` | `var(--color-text-disabled)` |
| `#ffffff` | `var(--color-text-inverse)` |

**Bordes:**

| Figma | Token |
|-------|-------|
| `rgba(41,81,54, 0.12)` | `var(--color-border)` / clase `.border-subtle` |
| `rgba(41,81,54, 0.25)` | `var(--color-border-strong)` |
| `rgba(41,81,54, 0.05)` | `var(--color-border-list)` |

**Estados de verificación:**

| Estado | Fondo | Borde | Texto |
|--------|-------|-------|-------|
| Verificado | `var(--color-verified-bg)` | `var(--color-verified-border)` | `var(--color-verified-text)` |
| Activo | `var(--color-active-bg)` | `var(--color-active-border)` | `var(--color-active-text)` |
| Pendiente | `var(--color-pending-bg)` | `var(--color-pending-border)` | `var(--color-pending-text)` |
| No verificado | `var(--color-unverified-bg)` | — | `var(--color-rejection)` |

### Espaciado Figma → Tokens

| Rango Figma (px) | Token BM | Alias |
|-------------------|----------|-------|
| 1 – 4 | `var(--spacing-1)` = 4px | `--spacing-xs` |
| 5 – 8 | `var(--spacing-2)` = 8px | `--spacing-sm` |
| 9 – 12 | `var(--spacing-3)` = 12px | — |
| 13 – 16 | `var(--spacing-4)` = 16px | `--spacing-md` |
| 17 – 20 | `var(--spacing-5)` = 20px | — |
| 21 – 24 | `var(--spacing-6)` = 24px | `--spacing-lg` |
| 25 – 32 | `var(--spacing-8)` = 32px | `--spacing-xl` |
| 33 – 40 | `var(--spacing-10)` = 40px | — |
| 41 – 48 | `var(--spacing-12)` = 48px | `--spacing-2xl` |

### Tipografía Figma → Tokens

| Figma (px) | Token BM | Nota |
|------------|----------|------|
| 9px | `var(--font-size-9)` | Micro texto |
| 10px | `var(--font-size-10)` | Captions pequeños |
| 11px | `var(--font-size-11)` | Labels compactos |
| 12px | `var(--font-size-xs)` | Texto extra small |
| 13px | `var(--font-size-13)` | Meta info |
| 14px | `var(--font-size-sm)` | Texto estándar pequeño |
| 16px | `var(--font-size-base)` | Texto base |
| 18px | `var(--font-size-lg)` | Subtítulos |
| 20px | `var(--font-size-xl)` | Títulos medianos |
| 24px | `var(--font-size-2xl)` | Títulos grandes |
| 30px | `var(--font-size-3xl)` | Headings principales |

**Pesos:**
| Figma | Token |
|-------|-------|
| Light / 300 | `var(--font-weight-light)` |
| Regular / 400 | `var(--font-weight-regular)` |
| Medium / 500 | `var(--font-weight-medium)` |
| SemiBold / 600 | `var(--font-weight-semibold)` |
| Bold / 700 | `var(--font-weight-bold)` |

**Familias:**
- Body / general → `var(--font-family-base)` = Inter
- Headings / acentos → Sora (aplicar manualmente `font-family: 'Sora'`)
- Código → `var(--font-family-mono)`

### Border Radius Figma → Tokens

| Figma (px) | Token BM |
|------------|----------|
| 2px | `var(--radius-xs)` |
| 4px | `var(--radius-sm)` |
| 8px | `var(--radius-md)` |
| 12px | `var(--radius-lg)` |
| 16px | `var(--radius-xl)` |
| 24px | `var(--radius-2xl)` |
| 9999px / pill | `var(--radius-full)` |

### Sombras Figma → Tokens

| Figma (descripción visual) | Token BM |
|----------------------------|----------|
| Sombra mínima | `var(--shadow-xs)` |
| Sombra suave/difusa | `var(--shadow-soft)` |
| Sombra de card | `var(--shadow-card)` |
| Sombra hover/elevada | `var(--shadow-hover)` |
| Sombra de modal | `var(--shadow-modal)` |
| Sombra pequeña neutra | `var(--shadow-sm)` |
| Sombra de botón | `var(--shadow-btn)` |
| Sombra de acción | `var(--shadow-action)` |

### Transiciones

| Uso | Token BM |
|-----|----------|
| Micro-interacción (hover icon) | `var(--transition-quick)` = 100ms |
| Hover estándar | `var(--transition-fast)` = 150ms |
| Animación general | `var(--transition-base)` = 250ms |
| Animación lenta (modals) | `var(--transition-slow)` = 400ms |
| Bounce (feedback) | `var(--transition-bounce)` = 300ms cubic-bezier |

---

## 🧩 CATÁLOGO DE COMPONENTES DS

### Botones `.ds-btn`
```html
<!-- Primary -->
<button class="ds-btn ds-btn--primary">
    <span class="material-symbols-outlined">add</span>
    Crear
</button>

<!-- Ghost icon-only -->
<button class="ds-btn ds-btn--ghost ds-btn--icon ds-btn--sm" title="Editar">
    <span class="material-symbols-outlined">edit</span>
</button>

<!-- Loading state -->
<button class="ds-btn ds-btn--primary is-loading" disabled>Guardando</button>
```

| Variante | Clase |
|----------|-------|
| Primary | `.ds-btn--primary` |
| Secondary (outline) | `.ds-btn--secondary` |
| Ghost | `.ds-btn--ghost` |
| Danger | `.ds-btn--danger` |
| Success | `.ds-btn--success` |
| **Tamaños:** sm / default / lg | `.ds-btn--sm` / (nada) / `.ds-btn--lg` |
| Icon-only | `.ds-btn--icon` |

### Cards `.ds-card`
```html
<div class="ds-card ds-card--hoverable">
    <div class="ds-card__header">
        <h3 class="ds-card__title">Título</h3>
        <p class="ds-card__subtitle">Subtítulo</p>
    </div>
    <div class="ds-card__body"><!-- contenido --></div>
    <div class="ds-card__footer"><!-- acciones --></div>
</div>
```

Variantes: `--hoverable`, `--flat`, `--glass`. Sub-componente: `.ds-stat-card`.

### Badges `.ds-badge`
```html
<span class="ds-badge ds-badge--success ds-badge--dot">Activo</span>
<span class="ds-badge ds-badge--danger">Bloqueado</span>
<span class="ds-badge ds-badge--count">5</span>
```

Variantes: `--success`, `--warning`, `--danger`, `--info`, `--alert`, `--primary`, `--neutral`.
Modificadores: `--dot`, `--sm`, `--lg`, `--count`.

### Tablas `.ds-table`
```html
<div class="ds-table-wrapper">
    <table class="ds-table">
        <thead><tr><th data-sortable>Col</th></tr></thead>
        <tbody><!-- filas --></tbody>
    </table>
</div>
```

Classes especiales: `.ds-table__col--actions`, `.ds-table__col--number`, `.ds-table__col--center`, `tr.is-selected`.

### Formularios
| Elemento | Clase |
|----------|-------|
| Grupo | `.ds-form-group` |
| Label | `.ds-label` (+ `.ds-label--required` para asterisco) |
| Input | `.ds-input` |
| Select | `.ds-select` |
| Textarea | `.ds-textarea` |
| Input con icono | `.ds-input-group` > `.ds-input-group__icon` + `.ds-input` |
| Error | `.ds-field-error` (visible con `.is-invalid` en input) |
| Hint | `.ds-field-hint` |
| Validación | `.is-valid` / `.is-invalid` (agregadas por `DS.form.validate()`) |

### Feedback
- **Alerts:** `.ds-alert .ds-alert--{success|warning|danger|info}` con `__icon`, `__body`, `__title`, `__close`.
- **Toasts:** Vía JS → `DS.notify.success('msg')` / `.error()` / `.warning()` / `.info()`.
- **Modales:** `.ds-modal-backdrop` > `.ds-modal .ds-modal--{sm|lg|xl}` con `__header`, `__body`, `__footer`. Abrir con `DS.modal.open('id')`.

### Layout
- **Page Header:** `.ds-page-header` > `__left` (title + subtitle) + `__right` (actions). Partial: `_DsPageHeader.cshtml`.
- **Toolbar:** `.ds-toolbar` > `__left` + `__right`.
- **Empty State:** `.ds-empty` > `__icon` + `__title` + `__description`.
- **Spinner:** `.ds-spinner` (+ `.ds-spinner--sm`).
- **Divider:** `.ds-divider`.

### Search & Client List
- **Search Widget:** `.ds-search-widget` > `.ds-search-bar` + `.ds-search-filters` + `.ds-search-dropdown`.
- **Client List:** `.ds-client-list` > `__header` + `__body` con `.ds-client-article` items.
- **Contact Icons:** `.ds-ci .ds-ci--{verified|pending|error}`.

---

## 🧩 PARTIALS RAZOR DISPONIBLES

| Partial | ViewData Keys | Uso |
|---------|---------------|-----|
| `_DsPageHeader` | `_DsPageHeader_Title`, `_DsPageHeader_Subtitle` | Encabezado de página + slot PageActions |
| `_DsBadge` | `_DsBadge_Text`, `_DsBadge_Variant`, `_DsBadge_Dot`, `_DsBadge_Size` | Badge reutilizable |
| `_DsCard` | `_DsCard_Title`, `_DsCard_Subtitle`, `_DsCard_Hoverable`, `_DsCard_Class` | Card con slot CardActions + body |
| `_DsModal` | `_DsModal_Id`, `_DsModal_Title`, `_DsModal_Size` | Modal declarativo con footer default |
| `_DsClientSearch` | `SearchFilter`, `TotalTodos`, `TotalPorVerif`, `TotalVerif`, `SearchPlaceholder` | Barra búsqueda global + dropdown + pills |
| `_DsClientList` | `ClientListTitle`, `ClientListCount`, `ClientListTotal`, `ClientListSearch` | Panel lateral lista clientes |

---

## ⚡ MÓDULOS JS — API Rápida

| Módulo | Métodos clave |
|--------|---------------|
| `DS.api` | `.get(url)`, `.post(url, body)`, `.put()`, `.patch()`, `.delete()` — auto-CSRF, manejo 401, retorna `{ ok, data, error }` |
| `DS.notify` | `.success(msg)`, `.error(msg)`, `.warning(msg)`, `.info(msg)`, `.confirm(msg, opts)` |
| `DS.modal` | `.open(id)`, `.close()`, `.closeAll()`, `.create(opts)` — stack con focus trap |
| `DS.form` | `.validate(sel)`, `.serialize(sel)`, `.submitAjax(sel, opts)`, `.fill(sel, data)`, `.reset(sel)`, `.setLoading(sel, bool)` |
| `DS.events` | `.on(event, fn)`, `.off(event, fn)`, `.emit(event, data)`, `.once(event, fn)` |
| `DS.utils` | `.debounce()`, `.throttle()`, `.escapeHtml()`, `.formatDate()`, `.formatCurrency()`, `.qs()`, `.qsAll()` |
| `DS.state` | `.set(key, val)`, `.get(key)`, `.getAll()` — emite `state:{key}` al cambiar |
| `DS.search` | `.init()`, `.reload()` — auto-init, búsqueda global + sidebar |

**Escaping obligatorio:** Siempre usar `DS.utils.escapeHtml()` al insertar datos de usuario en el DOM.

---

## 🚫 ALIASES LEGACY — NO USAR EN CÓDIGO NUEVO

Estas variables existen por compatibilidad. Si Figma exporta valores que coincidan, **mapear al token moderno**:

| Variable legacy | Usar en su lugar |
|---|---|
| `--forest-green` | `var(--color-primary-dark)` |
| `--background-light` | `var(--color-bg-light)` |
| `--verde-musgo` | `var(--color-primary-light)` |
| `--ocre-intenso` | `var(--color-alert)` |
| `--alert-ochre` | `var(--color-alert)` |
| `--success-green` | `var(--color-success)` |
| `--danger-red` | `var(--color-danger)` |
| Clases Bootstrap `.btn-*` | `.ds-btn .ds-btn--*` |
| Clases Bootstrap `.form-control` | `.ds-input` / `.ds-select` |
| Clases Bootstrap `.alert-*` | `.ds-alert .ds-alert--*` |

---

## 📐 ESTRUCTURA RAZOR DE PÁGINA NUEVA

```html
@page
@model MiPaginaModel
@{ ViewData["Title"] = "Mi Página"; }

@section Styles {
    <link rel="stylesheet" href="~/css/mipagina.css" />
}

<partial name="Shared/Components/_DsPageHeader"
         view-data='@(new ViewDataDictionary(ViewData) {
             ["_DsPageHeader_Title"] = "Mi Página",
             ["_DsPageHeader_Subtitle"] = "Descripción"
         })' />

<!-- Contenido principal con componentes DS -->
<div class="ds-card">
    <div class="ds-card__body">
        <!-- ... -->
    </div>
</div>

@section Scripts {
    <script src="~/js/mipagina.js"></script>
}
```

---

## 🗺️ PLAN DE IMPLEMENTACIÓN BM

> ⚠️ **ESTE PLAN SERÁ EJECUTADO POR UN MODELO OBRERO (Haiku o equivalente).**
> El modelo generador es responsable de resolver TODOS los campos antes de entregar el plan.
> El implementador **NO interpreta, NO decide, NO infiere** — solo lee y ejecuta.
>
> **OBLIGATORIO antes de entregar el plan:**
> - ✅ Todas las rutas son exactas (basadas en la estructura real del proyecto BM, no en `[Area]`).
> - ✅ Todos los SÍ/NO de dependencias están resueltos con motivo concreto basado en el código generado.
> - ✅ El código de cada archivo está **completo** — ningún `...`, `// resto`, ni fragmento parcial.
> - ✅ Cada paso de verificación indica: acción específica + resultado exacto y medible esperado.
> - ✅ Cero ambigüedad: si hay dos interpretaciones posibles, la instrucción está mal escrita.

El agente debe entregar el plan con exactamente la siguiente estructura, con **todos los campos resueltos**:

---

### 📋 PASO 1 — Archivos a crear o modificar
> Tabla con acción CREAR o MODIFICAR, ruta completa dentro del proyecto BM, y descripción del cambio.
> No usar rutas genéricas como `Pages/[Area]` — escribir la ruta real.

| Acción   | Ruta exacta                                           | Qué contiene / qué se cambia               |
|----------|-------------------------------------------------------|--------------------------------------------|
| CREAR    | Pages/Area/NombrePagina.cshtml                        | Vista del componente                       |
| CREAR    | Pages/Area/NombrePagina.cshtml.cs                     | PageModel con propiedades y handlers       |
| CREAR    | wwwroot/css/nombrepagina.css                          | Estilos específicos de esta vista          |
| CREAR    | wwwroot/js/nombrepagina.js                            | Lógica JS de esta vista                    |
| MODIFICAR| Pages/Shared/_Layout.cshtml                           | Solo si agrega dependencia global nueva    |

---

### 📄 PASO 2 — Contenido completo de cada archivo
> Entregar el código fuente **completo** de cada archivo listado en el Paso 1.
> El implementador copiará y pegará el contenido exactamente — sin leer ni interpretar más.
> **No omitir nada con `...` o comentarios de tipo `// resto del código`.**

**Pages/Area/NombrePagina.cshtml** — contenido completo:
```razor
{{CÓDIGO RAZOR COMPLETO}}
```

**Pages/Area/NombrePagina.cshtml.cs** — contenido completo:
```csharp
{{CÓDIGO C# COMPLETO DEL PAGEMODEL}}
```

**wwwroot/css/nombrepagina.css** — contenido completo:
```css
{{CSS COMPLETO}}
```

**wwwroot/js/nombrepagina.js** — contenido completo:
```js
{{JS COMPLETO}}
```

---

### 🎨 PASO 3 — Dependencias CSS
> Estado actual en `_Layout.cshtml`. Si falta alguna, escribir la línea HTML exacta a insertar.

| Archivo           | Estado                | Acción requerida (si falta, escribir la línea exacta)                        |
|-------------------|-----------------------|------------------------------------------------------------------------------|
| variables.css     | ✅ Ya presente         | Ninguna                                                                      |
| ds/ds.css         | ✅ Ya presente         | Ninguna                                                                      |
| nombrepagina.css  | ✅ Via @section Styles | Ya incluido en el .cshtml del Paso 2                                         |

Si hay una hoja que no está presente, escribir la etiqueta exacta:
`<link rel="stylesheet" href="~/css/[ruta-exacta].css" />`

---

### ⚡ PASO 4 — Dependencias JS
> Lista definitiva y resuelta. Sin condicionales — cada módulo tiene SÍ o NO con motivo concreto
> basado en el código generado (no en abstracciones).

| Módulo DS   | ¿Requerido? | Motivo concreto (basado en el código del Paso 2)                              |
|-------------|-------------|-------------------------------------------------------------------------------|
| DS.api      | SÍ / NO     | Ej: "Realiza POST a /api/[ruta] en el submit del formulario de ..."           |
| DS.notify   | SÍ / NO     | Ej: "Llama DS.notify.success() tras respuesta 200 de DS.api.post()"           |
| DS.modal    | SÍ / NO     | Ej: "Abre modal con DS.modal.open('modal-eliminar') al hacer clic en Eliminar"|
| DS.form     | SÍ / NO     | Ej: "Valida el formulario #form-[id] con DS.form.validate() antes de submit"  |
| DS.search   | SÍ / NO     | Ej: "No usa sidebar ni búsqueda global"                                       |
| DS.state    | SÍ / NO     | Ej: "Guarda filtro activo con DS.state.set('filtro', valor)"                  |
| DS.events   | SÍ / NO     | Ej: "Escucha evento 'cliente:actualizado' emitido por otro módulo"            |

---

### 🧩 PASO 5 — PageModel / ViewModel
> Propiedades exactas que la vista necesita. Especificar nombre, tipo C#, si tiene `[BindProperty]`
> y el origen del dato. No usar tipos genéricos como `object` o `dynamic`.

```csharp
// Propiedades necesarias en Pages/Area/NombrePagina.cshtml.cs

[BindProperty]
public string PropiedadDeFormulario { get; set; }    // Dato de input del formulario

public List<TipoConcreto> Items { get; set; }         // Cargado en OnGet desde _servicio.Metodo()
public int TotalItems => Items?.Count ?? 0;           // Calculado
```

| Propiedad             | Tipo C#           | [BindProperty] | Origen del dato                          |
|-----------------------|-------------------|----------------|------------------------------------------|
| PropiedadDeFormulario | string            | ✅ Sí           | Input del formulario                     |
| Items                 | List\<TipoConcreto\>| ❌ No         | `_servicio.ObtenerTodos()` en OnGet      |
| TotalItems            | int               | ❌ No           | Calculado: Items.Count                   |

---

### ✅ PASO 6 — Verificación ejecutable
> Cada ítem indica qué hacer **exactamente** y qué resultado **exacto y observable** esperar.
> No escribir "verificar que funcione" — escribir la acción y el resultado medible.

```
- [ ] Navegar a https://localhost:[puerto]/[ruta-exacta] — la página carga sin errores 500 ni redirección inesperada.
- [ ] Hacer hover sobre [elemento exacto, ej: botón "Guardar"] — fondo cambia a var(--color-primary-light) con transición visible.
- [ ] Hacer click en [acción exacta, ej: botón "Eliminar cliente"] — aparece modal con título "[texto exacto del título]".
- [ ] Confirmar en el modal — aparece toast verde con mensaje "[texto exacto esperado]" y el ítem desaparece de la lista.
- [ ] Dejar campo [nombre del campo] vacío y hacer submit — aparece .ds-field-error debajo del campo con texto "[mensaje de error esperado]".
- [ ] Ingresar `<script>alert(1)</script>` en [campo de texto] — el texto se muestra escapado en pantalla, el script no se ejecuta.
- [ ] Redimensionar ventana a 360px de ancho — [elemento específico] apila verticalmente sin overflow horizontal visible.
- [ ] Presionar Tab repetidamente — el foco visible recorre todos los controles interactivos en orden lógico de lectura.
- [ ] Abrir DevTools > Elements, seleccionar cualquier elemento DS — las clases existen en variables.css y/o ds.css (sin inventadas).
- [ ] Abrir wwwroot/css/nombrepagina.css — no hay ningún valor hexadecimal (#) ni rgba() hardcodeado.
```

---

## �📥 INPUT DATA (FIGMA RAW)
[PEGAR AQUÍ EL CÓDIGO EXPORTADO DE FIGMA O CSS INLINE]
