# Design System — Smart Hub Enterprise v2.0

> Documentación completa y actualizada del sistema de diseño.
> Última revisión: Marzo 2026.
>
> **Propósito:** Guía de referencia definitiva para mantener consistencia visual, de comportamiento
> y de código en cada pantalla nueva. Escrita para ser clara tanto para desarrolladores humanos
> como para asistentes de IA de cualquier tamaño.

---

## Tabla de contenidos

1. [Arquitectura de archivos](#1-arquitectura-de-archivos)
2. [Orden de carga CSS y JS](#2-orden-de-carga-css-y-js)
3. [Reglas de oro](#3-reglas-de-oro)
4. [Design Tokens (variables.css)](#4-design-tokens)
5. [Componentes CSS](#5-componentes-css)
   - 5.1 Botones · 5.2 Cards · 5.3 Badges · 5.4 Tablas · 5.5 Formularios
   - 5.6 Feedback (alerts, toasts, modals) · 5.7 Layout · 5.8 Search & Client List
6. [Módulos JS del Design System](#6-módulos-js-del-design-system)
   - 6.1 DS.events · 6.2 DS.utils · 6.3 DS.state · 6.4 DS.modules
   - 6.5 DS.notify · 6.6 DS.api · 6.7 DS.modal · 6.8 DS.form · 6.9 DS.search
7. [Componentes Razor (Partials)](#7-componentes-razor-partials)
8. [Sistema de iconos](#8-sistema-de-iconos)
9. [Integración con Tailwind CSS](#9-integración-con-tailwind-css)
10. [Patrón recomendado por pantalla nueva](#10-patrón-recomendado-por-pantalla-nueva)
11. [Tokens de referencia rápida](#11-tokens-de-referencia-rápida)
12. [Notas de migración y legado](#12-notas-de-migración-y-legado)

---

## 1. Arquitectura de archivos

```
wwwroot/
├── css/
│   ├── variables.css                 ← Tokens centralizados (colores, tipografía, espaciado, sombras, z-index, etc.)
│   ├── ds/
│   │   ├── ds.css                    ← Punto de entrada — importa los 8 archivos de componentes
│   │   └── components/
│   │       ├── _buttons.css          ← .ds-btn (primary, secondary, ghost, danger, success, tamaños, icon-only, loading)
│   │       ├── _cards.css            ← .ds-card (hoverable, flat, glass) + .ds-stat-card
│   │       ├── _badges.css           ← .ds-badge (success, warning, danger, info, alert, primary, neutral, dot, count)
│   │       ├── _tables.css           ← .ds-table-wrapper + .ds-table (sticky header, sortable, skeleton)
│   │       ├── _forms.css            ← .ds-form-group, .ds-input, .ds-select, .ds-textarea, validación
│   │       ├── _feedback.css         ← .ds-alert, .ds-toast, .ds-modal (con backdrop, tamaños, animaciones)
│   │       ├── _layout.css           ← .ds-page-header, .ds-toolbar, .ds-empty, .ds-divider, .ds-spinner
│   │       └── _search.css           ← .ds-search-bar, .ds-search-filters, .ds-search-dropdown,
│   │                                    .ds-client-list, .ds-client-article, .ds-ci (contact icons)
│   ├── bootstrap-overrides.css       ← Sobreescrituras de Bootstrap (.btn-forest-green, etc.)
│   ├── site.css                      ← Reset base y tamaño de fuente raíz
│   ├── layout.css                    ← .navbar-custom, body flex layout, footer
│   ├── tailwind.output.css           ← Resultado de Tailwind (clases con prefijo tw-)
│   └── [pagina].css                  ← CSS específico por página (cargado en @section Styles)
│
├── js/
│   ├── ds/
│   │   ├── ds-core.js                ← window.DS namespace: DS.events, DS.utils, DS.state, DS.modules
│   │   ├── ds-notifications.js       ← DS.notify (toasts: success/error/warning/info + confirm dialog)
│   │   ├── ds-api.js                 ← DS.api (get/post/put/patch/delete con auto-CSRF y manejo de 401)
│   │   ├── ds-modal.js               ← DS.modal (create/open/close/closeAll con stack y focus trap)
│   │   ├── ds-form.js                ← DS.form (validate, serialize, submitAjax, fill, reset, setLoading)
│   │   └── ds-search.js              ← DS.search (búsqueda global, dropdown, client list aside, contact icons)
│   └── [pagina].js                   ← Lógica específica de cada página
│
├── icons/                            ← Iconos SVG custom del proyecto (19 archivos)
│   ├── article-open.svg
│   ├── contact-email.svg
│   ├── contact-location.svg
│   ├── contact-phone.svg
│   ├── document-upload.svg
│   ├── edit.svg
│   ├── error-info.svg
│   ├── note-remove-active.svg
│   ├── note-remove.svg
│   ├── row-email.svg
│   ├── row-location.svg
│   ├── row-phone.svg
│   ├── row-phone2.svg
│   ├── save.svg
│   ├── search.svg
│   ├── section-email.svg
│   ├── section-location.svg
│   ├── section-phone.svg
│   └── verify-check.svg
│
└── lib/
    └── bootstrap/dist/css/           ← Bootstrap CSS (legacy, en migración)

Pages/
└── Shared/
    └── Components/
        ├── _DsPageHeader.cshtml      ← Encabezado estándar de página (Title, Subtitle, PageActions)
        ├── _DsBadge.cshtml           ← Badge / pill de estado (Text, Variant, Dot, Size)
        ├── _DsCard.cshtml            ← Card reutilizable (Title, Subtitle, Hoverable, Class)
        ├── _DsModal.cshtml           ← Modal declarativo (Id, Title, Size)
        ├── _DsClientSearch.cshtml    ← Barra de búsqueda global + dropdown + pills de filtro
        └── _DsClientList.cshtml      ← Panel lateral de lista de clientes con búsqueda interna
```

---

## 2. Orden de carga CSS y JS

### CSS — definido en `_Layout.cshtml`

El orden es **crítico** para la cascada de especificidad. Se carga así:

| # | Archivo | Propósito |
|---|---------|-----------|
| 1 | **Google Fonts** (link) | Inter (body), Sora (headings/accents), Material Icons, Material Symbols Outlined |
| 2 | **Bootstrap CSS** (`lib/bootstrap/dist/css/bootstrap.min.css`) | Framework base legacy |
| 3 | **Tailwind output** (`css/tailwind.output.css`) | Clases utilitarias con prefijo `tw-` |
| 4 | **Design Tokens** (`css/variables.css`) | Variables CSS custom properties en `:root` |
| 5 | **Design System** (`css/ds/ds.css`) | Importa los 8 archivos de componentes DS |
| 6 | **Bootstrap Overrides** (`css/bootstrap-overrides.css`) | Sobreescrituras de Bootstrap con tokens DS |
| 7 | **Site** (`css/site.css`) | Reset base, tamaños de fuente raíz |
| 8 | **Layout** (`css/layout.css`) | Navbar `.navbar-custom`, body flex, footer |
| 9 | **`@section Styles`** | CSS específico de cada página Razor |

### JS — definido en `_Layout.cshtml`

| # | Archivo | Propósito |
|---|---------|-----------|
| 1 | `js/ds/ds-core.js` | Crea `window.DS`, EventBus, utils, state, modules. Auto-llama `DS.modules.initAll()` en DOMContentLoaded |
| 2 | `js/ds/ds-notifications.js` | Registra módulo 'notifications', expone `DS.notify` |
| 3 | `js/ds/ds-api.js` | Registra módulo 'api', expone `DS.api` |
| 4 | `js/ds/ds-modal.js` | Registra módulo 'modal', expone `DS.modal` |
| 5 | `js/ds/ds-form.js` | Registra módulo 'form', expone `DS.form` |
| 6 | `js/ds/ds-search.js` | IIFE auto-init, expone `DS.search` |
| 7 | **`@section Scripts`** | JS específico de cada página Razor |

### Tipografías cargadas

- **Inter** — Pesos 300, 400, 500, 600, 700. Fuente principal para cuerpo de texto.
- **Sora** — Pesos 400, 500, 600, 700. Fuente alternativa para headings y acentos.
- **Material Icons** — Iconos legacy (clase `material-icons`).
- **Material Symbols Outlined** — Iconos principales (clase `material-symbols-outlined`).

### Analytics

Se incluye un script de **Microsoft Clarity** al final del `<body>` para tracking de UX.

---

## 3. Reglas de oro

| # | Regla | Qué hacer |
|---|-------|-----------|
| 1 | **Tokens siempre** | Nunca hardcodear colores, tamaños o sombras. Usar `var(--color-...)`, `var(--spacing-...)`, `var(--shadow-...)` |
| 2 | **Componente DS primero** | Antes de escribir nuevo CSS, buscar si existe `.ds-card`, `.ds-btn`, `.ds-badge`, etc. |
| 3 | **CSS por página → `@section Styles`** | Nunca agregar CSS de una página específica al `_Layout.cshtml` global |
| 4 | **JS centralizado** | Usar `DS.api`, `DS.notify`, `DS.modal`, `DS.form` en lugar de fetch/alert/confirm directo |
| 5 | **Eventos en lugar de acoplamiento** | Comunicar entre módulos con `DS.events.emit()` / `DS.events.on()` |
| 6 | **Un archivo JS por página** | La lógica de `Dashboard.cshtml` → `dashboard.js`, de `Clientes.cshtml` → `clientes.js` |
| 7 | **Escaping HTML obligatorio** | Siempre usar `DS.utils.escapeHtml()` al insertar datos de usuario en el DOM |
| 8 | **SVG como `<img>` con clase `ds-icon`** | Los iconos SVG custom se cargan como `<img src="~/icons/nombre.svg" class="ds-icon">` |
| 9 | **Tailwind solo para componentes aislados** | Usar clases `tw-*` solamente para componentes específicos. El DS propio tiene prioridad. |

---

## 4. Design Tokens

Definidos en `wwwroot/css/variables.css`. Todas las variables viven en `:root`.

### 4.1 Paleta principal

| Token | Valor | Uso |
|-------|-------|-----|
| `--color-primary` | `#446942` | Verde principal de la marca |
| `--color-primary-light` | `#7F9787` | Versión suave para hover/acentos |
| `--color-primary-dark` | `#295136` | Versión oscura para texto/énfasis |
| `--color-primary-50` | `rgba(68,105,66, 0.05)` | Fondo ultra-sutil |
| `--color-primary-10` | `rgba(68,105,66, 0.10)` | Fondo sutil |

**Opacidades de primary-dark** (rgb 41,81,54):

`--color-pd-04`, `--color-pd-05`, `--color-pd-10`, `--color-pd-18`, `--color-pd-20`, `--color-pd-60`

### 4.2 Fondos

| Token | Valor | Uso |
|-------|-------|-----|
| `--color-bg-light` | `#DBE4DD` | Fondo claro decorativo |
| `--color-bg-dark` | `#171b17` | Fondo oscuro |
| `--color-bg-page` | `#f5f7f5` | Fondo de la página principal |
| `--color-bg-surface` | `#ffffff` | Superficie de cards, modales |
| `--color-bg-subtle` | `#eef2ee` | Fondo sutil |
| `--color-bg-selected` | `#ebf2ee` | Fila activa en listas de clientes |
| `--color-neutral-100` | `#F3F4F6` | Gris neutro para fondos |
| `--color-overlay` | `rgba(0,0,0, 0.35)` | Overlay de modales |

### 4.3 Estados

| Token | Valor | Fondo asociado |
|-------|-------|----------------|
| `--color-alert` | `#CB801B` | `--color-alert-bg: rgba(203,128,27, 0.12)` |
| `--color-success` | `#28a745` | `--color-success-bg`, `--color-success-20`, `--color-success-25` |
| `--color-warning` | `#ffc107` | `--color-warning-bg: rgba(255,193,7, 0.15)` |
| `--color-danger` | `#dc3545` | `--color-danger-bg`, `--color-danger-3/6/10/20/30/40` |
| `--color-info` | `#0dcaf0` | `--color-info-bg: rgba(13,202,240, 0.12)` |

### 4.4 Estados de verificación

| Token | Valor | Uso |
|-------|-------|-----|
| `--color-verified-bg` | `#F0FDF4` | Fondo de filas/badges verificados |
| `--color-verified-border` | `#BBF7D0` | Borde verificado |
| `--color-verified-text` | `#166534` | Texto verificado |
| `--color-active-bg` | `#DCFCE7` | Fondo activo |
| `--color-active-border` | `#BBF7D0` | Borde activo |
| `--color-active-text` | `#166534` | Texto activo |
| `--color-pending-bg` | `#FEF7E0` | Fondo pendiente |
| `--color-pending-border` | `#FEF08A` | Borde pendiente |
| `--color-pending-text` | `#B06000` | Texto pendiente |
| `--color-unverified-bg` | `#fef2f2` | Fondo no verificado |
| `--color-rejection` | `#dc2626` | Color de rechazo |

### 4.5 Textos

| Token | Valor |
|-------|-------|
| `--color-text-primary` | `#295136` |
| `--color-text-secondary` | `rgba(41,81,54, 0.70)` |
| `--color-text-muted` | `rgba(41,81,54, 0.50)` |
| `--color-text-disabled` | `rgba(41,81,54, 0.30)` |
| `--color-text-inverse` | `#ffffff` |

### 4.6 Bordes

| Token | Valor |
|-------|-------|
| `--color-border` | `rgba(41,81,54, 0.12)` |
| `--color-border-strong` | `rgba(41,81,54, 0.25)` |
| `--color-border-focus` | `var(--color-primary)` |
| `--color-border-list` | `rgba(41,81,54, 0.05)` — separador suave en listas |

### 4.7 Sombras

| Token | Valor | Uso |
|-------|-------|-----|
| `--shadow-xs` | `0 1px 3px rgba(41,81,54, 0.06)` | Sombra mínima |
| `--shadow-soft` | `0 4px 20px -2px rgba(41,81,54, 0.08)` | Sombra suave |
| `--shadow-card` | `0 2px 12px rgba(41,81,54, 0.04)` | Cards |
| `--shadow-hover` | `0 8px 24px rgba(41,81,54, 0.12)` | Hover elevado |
| `--shadow-modal` | `0 20px 60px rgba(41,81,54, 0.20)` | Modales |
| `--shadow-sm` | `0 1px 2px rgba(0,0,0, 0.05)` | Neutro pequeño |
| `--shadow-btn` | `0 2px 4px -2px rgba(0,0,0,.10), 0 4px 6px -1px rgba(0,0,0,.10)` | Botones |
| `--shadow-action` | `0 2px 4px rgba(0,0,0, 0.08)` | Acciones interactivas |

### 4.8 Border Radius

| Token | Valor |
|-------|-------|
| `--radius-xs` | `0.125rem` (2px) |
| `--radius-sm` | `0.25rem` (4px) |
| `--radius-md` | `0.5rem` (8px) |
| `--radius-lg` | `0.75rem` (12px) |
| `--radius-xl` | `1rem` (16px) |
| `--radius-2xl` | `1.5rem` (24px) |
| `--radius-full` | `9999px` (pill/circular) |

### 4.9 Espaciado

| Token | Valor | Alias semántico |
|-------|-------|-----------------|
| `--spacing-1` | `0.25rem` (4px) | `--spacing-xs` |
| `--spacing-2` | `0.5rem` (8px) | `--spacing-sm` |
| `--spacing-3` | `0.75rem` (12px) | — |
| `--spacing-4` | `1rem` (16px) | `--spacing-md` |
| `--spacing-5` | `1.25rem` (20px) | — |
| `--spacing-6` | `1.5rem` (24px) | `--spacing-lg` |
| `--spacing-8` | `2rem` (32px) | `--spacing-xl` |
| `--spacing-10` | `2.5rem` (40px) | — |
| `--spacing-12` | `3rem` (48px) | `--spacing-2xl` |

### 4.10 Tipografía

| Token | Valor |
|-------|-------|
| `--font-family-base` | `'Inter', system-ui, sans-serif` |
| `--font-family-mono` | `'Liberation Mono', 'Courier New', monospace` |
| **Tamaños** | |
| `--font-size-9` | `0.5625rem` (9px) |
| `--font-size-10` | `0.625rem` (10px) |
| `--font-size-11` | `0.6875rem` (11px) |
| `--font-size-13` | `0.8125rem` (13px) |
| `--font-size-xs` | `0.75rem` (12px) |
| `--font-size-sm` | `0.875rem` (14px) |
| `--font-size-base` | `1rem` (16px) |
| `--font-size-lg` | `1.125rem` (18px) |
| `--font-size-xl` | `1.25rem` (20px) |
| `--font-size-2xl` | `1.5rem` (24px) |
| `--font-size-3xl` | `1.875rem` (30px) |
| `--font-size-4xl` | `3.5rem` (56px — iconos decorativos) |
| **Pesos** | |
| `--font-weight-light` | `300` |
| `--font-weight-regular` | `400` |
| `--font-weight-medium` | `500` |
| `--font-weight-semibold` | `600` |
| `--font-weight-bold` | `700` |
| **Line Heights** | |
| `--line-height-tight` | `1.25` |
| `--line-height-normal` | `1.5` |
| `--line-height-loose` | `1.75` |

### 4.11 Transiciones

| Token | Valor |
|-------|-------|
| `--transition-quick` | `100ms ease` |
| `--transition-fast` | `150ms ease` |
| `--transition-base` | `250ms ease` |
| `--transition-slow` | `400ms ease` |
| `--transition-medium` | `600ms ease` |
| `--transition-bounce` | `300ms cubic-bezier(0.34, 1.56, 0.64, 1)` |

### 4.12 Z-Index

| Token | Valor | Uso |
|-------|-------|-----|
| `--z-base` | `0` | Elementos normales |
| `--z-raised` | `10` | Elementos elevados |
| `--z-dropdown` | `100` | Dropdowns, menús |
| `--z-sticky` | `200` | Headers/navs sticky |
| `--z-overlay` | `300` | Overlays |
| `--z-modal` | `400` | Modales |
| `--z-toast` | `500` | Notificaciones toast |
| `--z-top` | `1000` | Elementos sobre todo |

### 4.13 Dimensiones de layout

| Token | Valor |
|-------|-------|
| `--navbar-height` | `64px` |
| `--aside-width` | `426px` |

### 4.14 Tokens de componentes

**Botones:**
`--btn-height-sm` (30px), `--btn-height-md` (36px), `--btn-height-lg` (44px),
`--btn-padding-x`, `--btn-radius` (full), `--btn-font-size` (sm), `--btn-font-weight` (semibold)

**Cards:**
`--card-padding` (spacing-6), `--card-radius` (xl), `--card-shadow` (shadow-card), `--card-border`

**Inputs:**
`--input-height` (2.5rem), `--input-padding-x`, `--input-radius` (lg),
`--input-border`, `--input-border-neutral` (#D1D5DB), `--input-border-focus`,
`--input-font-size` (sm), `--input-placeholder-color` (#6B7280)

### 4.15 Clases utilitarias (variables.css)

```css
.bg-glass        /* background blanco 90% + backdrop-filter blur */
.text-primary    /* color: var(--color-text-primary) */
.text-secondary  /* color: var(--color-text-secondary) */
.text-muted      /* color: var(--color-text-muted) */
.border-subtle   /* border-color: var(--color-border) */
.shadow-soft     /* box-shadow: var(--shadow-soft) */
.shadow-card     /* box-shadow: var(--shadow-card) */
.shadow-hover    /* box-shadow: var(--shadow-hover) */
```

---

## 5. Componentes CSS

Todos los componentes usan la convención BEM: `.ds-componente`, `.ds-componente__hijo`, `.ds-componente--modificador`.
Los archivos viven en `wwwroot/css/ds/components/` y se importan desde `ds.css`.

### 5.1 Botones (`_buttons.css`)

**Clase base:** `.ds-btn` — `display: inline-flex`, centrado, `height: var(--btn-height-md)`, border-radius pill, transiciones, `active: scale(0.97)`.

#### Variantes de color

| Clase | Estilo |
|-------|--------|
| `.ds-btn--primary` | Fondo `--color-primary`, texto blanco, sombra |
| `.ds-btn--secondary` | Outline `--color-primary-dark`, fondo transparente |
| `.ds-btn--ghost` | Sin borde, fondo transparente, hover sutil |
| `.ds-btn--danger` | Fondo `--color-danger`, texto blanco |
| `.ds-btn--success` | Fondo `--color-success`, texto blanco |

#### Tamaños

| Clase | Altura |
|-------|--------|
| `.ds-btn--sm` | 30px |
| (default) | 36px |
| `.ds-btn--lg` | 44px |

#### Modificadores especiales

| Clase | Efecto |
|-------|--------|
| `.ds-btn--icon` | Botón cuadrado (sin padding horizontal, width = height) |
| `.is-loading` | Oculta texto, muestra spinner con pseudo-elemento `::after` |

```html
<!-- Ejemplo completo -->
<button class="ds-btn ds-btn--primary">
    <span class="material-symbols-outlined">add</span>
    Crear cliente
</button>

<button class="ds-btn ds-btn--ghost ds-btn--icon ds-btn--sm" title="Editar">
    <span class="material-symbols-outlined">edit</span>
</button>

<button class="ds-btn ds-btn--primary is-loading" disabled>Guardando</button>
```

### 5.2 Cards (`_cards.css`)

**Clase base:** `.ds-card` — border, radius `--card-radius`, shadow `--card-shadow`, padding, transición.

#### Variantes

| Clase | Efecto |
|-------|--------|
| `.ds-card--hoverable` | Hover: sombra elevada + translateY(-2px) |
| `.ds-card--flat` | Sin sombra, solo borde |
| `.ds-card--glass` | Fondo semi-transparente + backdrop-filter blur |

#### Elementos internos

| Clase | Descripción |
|-------|-------------|
| `.ds-card__header` | Contenedor flex para título y acciones |
| `.ds-card__title` | `<h3>` del card |
| `.ds-card__subtitle` | Texto secundario debajo del título |
| `.ds-card__body` | Contenido principal |
| `.ds-card__footer` | Pie de card con borde superior |

#### Sub-componente: Stat Card

```html
<div class="ds-card ds-stat-card">
    <div class="ds-stat-card__icon">
        <span class="material-symbols-outlined">people</span>
    </div>
    <span class="ds-stat-card__value">1,234</span>
    <span class="ds-stat-card__label">Clientes totales</span>
    <span class="ds-stat-card__trend ds-stat-card__trend--up">↑ 12%</span>
</div>
```

Trends: `--up` (verde) | `--down` (rojo).

### 5.3 Badges (`_badges.css`)

**Clase base:** `.ds-badge` — inline-flex, pill shape, font-size xs, font-weight semibold.

#### Variantes de estado

| Clase | Color |
|-------|-------|
| `.ds-badge--success` | Verde |
| `.ds-badge--warning` | Amarillo |
| `.ds-badge--danger` | Rojo |
| `.ds-badge--info` | Cyan |
| `.ds-badge--alert` | Ocre/ámbar |
| `.ds-badge--primary` | Verde primario |
| `.ds-badge--neutral` | Gris |

#### Modificadores

| Clase | Efecto |
|-------|--------|
| `.ds-badge--dot` | Agrega un punto de color a la izquierda (pseudo-elemento `::before`) |
| `.ds-badge--sm` | Más pequeño |
| `.ds-badge--lg` | Más grande |
| `.ds-badge--count` | Circular numérico (para contadores) |

```html
<span class="ds-badge ds-badge--success ds-badge--dot">Activo</span>
<span class="ds-badge ds-badge--danger">Bloqueado</span>
<span class="ds-badge ds-badge--count">5</span>
```

### 5.4 Tablas (`_tables.css`)

**Estructura:**

```html
<div class="ds-table-wrapper">
    <table class="ds-table">
        <thead>
            <tr>
                <th data-sortable>Nombre</th>
                <th>Estado</th>
                <th class="ds-table__col--actions">Acciones</th>
            </tr>
        </thead>
        <tbody>
            <tr>
                <td>Juan Pérez</td>
                <td><span class="ds-badge ds-badge--success ds-badge--dot">Activo</span></td>
                <td class="ds-table__col--actions">
                    <button class="ds-btn ds-btn--ghost ds-btn--icon ds-btn--sm">
                        <span class="material-symbols-outlined">edit</span>
                    </button>
                </td>
            </tr>
        </tbody>
    </table>
</div>
```

**Características:**
- `.ds-table-wrapper`: overflow-x auto, borde, border-radius
- Header sticky (position: sticky, top: 0)
- Hover en filas: fondo sutil
- `tr.is-selected`: fila seleccionada visualmente
- `th[data-sortable]`: cursor pointer, pseudo-elementos para indicadores `aria-sort`
- `.ds-table__col--actions`: centrado, ancho mínimo
- `.ds-table__col--number`: alineado a la derecha
- `.ds-table__col--center`: centrado
- `.ds-table__empty`: estado vacío con icono

**Skeleton loader:**

```html
<div class="ds-skeleton-line" style="height:12px;width:120px"></div>
```

Animación `ds-skeleton-pulse` (1.6s ease-in-out infinite, opacidad pulsa entre 1 y 0.5).

### 5.5 Formularios (`_forms.css`)

**Clases principales:**

| Clase | Descripción |
|-------|-------------|
| `.ds-form-group` | Contenedor flex column con gap y margin-bottom |
| `.ds-label` | Etiqueta de campo |
| `.ds-label--required` | Agrega asterisco rojo (`::after`) |
| `.ds-input` | Input de texto |
| `.ds-select` | Select con flecha custom (SVG chevron) |
| `.ds-textarea` | Textarea con resize vertical |
| `.ds-input-group` | Input con icono a la izquierda |
| `.ds-input-group__icon` | Icono dentro del input-group |
| `.ds-field-error` | Mensaje de error (hidden por defecto, visible cuando `.is-invalid`) |
| `.ds-field-hint` | Texto de ayuda en gris |
| `.ds-check` | Checkbox/radio con `accent-color` |

**Validación (clases JS):**

- `.is-valid` → borde verde, focus ring verde
- `.is-invalid` → borde rojo, focus ring rojo + muestra `.ds-field-error`

```html
<div class="ds-form-group">
    <label class="ds-label ds-label--required" for="nombre">Nombre</label>
    <div class="ds-input-group">
        <span class="ds-input-group__icon material-symbols-outlined">person</span>
        <input id="nombre" name="nombre" class="ds-input" placeholder="Juan Pérez"
               required data-error-required="El nombre es obligatorio" />
    </div>
    <span class="ds-field-error"></span>
    <span class="ds-field-hint">Nombre completo del cliente</span>
</div>
```

### 5.6 Feedback (`_feedback.css`)

#### Alerts (inline)

```html
<div class="ds-alert ds-alert--warning">
    <span class="ds-alert__icon material-symbols-outlined">warning</span>
    <div class="ds-alert__body">
        <strong class="ds-alert__title">Atención</strong>
        <p>Hay campos sin completar.</p>
    </div>
    <button class="ds-alert__close">
        <span class="material-symbols-outlined">close</span>
    </button>
</div>
```

Variantes: `--success`, `--warning`, `--danger`, `--info`.

#### Toasts (notificaciones)

- Contenedor: `#ds-toast-container` — fixed, bottom: 24px, right: 24px, z-index: var(--z-toast)
- Elemento: `.ds-toast` — dark background, slide-in desde la derecha
- Partes: `.ds-toast__icon`, `.ds-toast__message`, `.ds-toast__progress`
- Estado salida: `.is-leaving` con animación de slide-out
- Variantes de icono por color: success (verde), error (rojo), warning (amarillo), info (cyan)
- **Se crean via JS:** `DS.notify.success('Mensaje')` — No usar HTML directo.

#### Modales

```html
<div class="ds-modal-backdrop" id="mi-modal" style="display:none" role="dialog" aria-modal="true">
    <div class="ds-modal ds-modal--lg">
        <div class="ds-modal__header">
            <h2 class="ds-modal__title">Título</h2>
            <button class="ds-modal__close" onclick="DS.modal.close()">
                <span class="material-symbols-outlined">close</span>
            </button>
        </div>
        <div class="ds-modal__body">
            <!-- contenido -->
        </div>
        <div class="ds-modal__footer">
            <button class="ds-btn ds-btn--ghost" onclick="DS.modal.close()">Cancelar</button>
            <button class="ds-btn ds-btn--primary" id="btn-confirmar">Confirmar</button>
        </div>
    </div>
</div>
```

**Tamaños:** (sin modificador = md), `--sm`, `--lg`, `--xl`.

**Animaciones:** `ds-fade-in` (backdrop), `ds-modal-in` (contenido).

**Abrir desde JS:** `DS.modal.open('mi-modal')`.

### 5.7 Layout (`_layout.css`)

#### Page Header

```html
<div class="ds-page-header">
    <div class="ds-page-header__left">
        <h1 class="ds-page-title">Clientes</h1>
        <p class="ds-page-subtitle">1,234 clientes registrados</p>
    </div>
    <div class="ds-page-header__right">
        <button class="ds-btn ds-btn--primary">
            <span class="material-symbols-outlined">add</span>
            Nuevo
        </button>
    </div>
</div>
```

#### Toolbar

```html
<div class="ds-toolbar">
    <div class="ds-toolbar__left">
        <div class="ds-input-group">
            <span class="ds-input-group__icon material-symbols-outlined">search</span>
            <input class="ds-input ds-input--sm" placeholder="Buscar..." />
        </div>
    </div>
    <div class="ds-toolbar__right">
        <select class="ds-select" style="width:auto">
            <option>Todos</option>
        </select>
    </div>
</div>
```

#### Empty State

```html
<div class="ds-empty">
    <span class="ds-empty__icon material-symbols-outlined">search_off</span>
    <h3 class="ds-empty__title">Sin resultados</h3>
    <p class="ds-empty__description">No se encontraron clientes con estos filtros.</p>
</div>
```

#### Otros elementos

| Clase | Descripción |
|-------|-------------|
| `.ds-divider` | Línea horizontal separadora |
| `.ds-loading-overlay` | Overlay absoluto con blur para secciones |
| `.ds-spinner` | Círculo de carga animado (border rotating) |
| `.ds-spinner--sm` | Versión pequeña del spinner |

### 5.8 Search & Client List (`_search.css`)

Este es el archivo más extenso (~600 líneas). Contiene todo el sistema de búsqueda de clientes y el panel lateral.

#### 5.8.1 Search Widget

**Contenedor:** `.ds-search-widget` — posición relativa, ancho completo. Envuelve la barra, filtros y dropdown.

#### 5.8.2 Search Bar

```html
<div class="ds-search-bar ds-search-bar--lg">
    <img src="~/icons/search.svg" class="ds-icon ds-search-bar__icon" aria-hidden="true" alt="" />
    <input type="search" class="ds-search-bar__input" placeholder="Búsqueda Global..." />
    <button class="ds-search-bar__clear" style="display:none">
        <span class="material-symbols-outlined">close</span>
    </button>
</div>
```

| Clase | Descripción |
|-------|-------------|
| `.ds-search-bar` | Flex container con borde, radius, focus-within ring |
| `.ds-search-bar--lg` | Versión grande (más padding) |
| `.ds-search-bar__icon` | Icono de lupa a la izquierda |
| `.ds-search-bar__input` | Input sin borde (transparent background) |
| `.ds-search-bar__clear` | Botón X para limpiar |

#### 5.8.3 Filter Pills

```html
<div class="ds-search-filters" role="tablist">
    <button class="ds-search-filters__pill ds-search-filters__pill--active"
            data-filter="todos" type="button">
        Todos (150)
    </button>
    <button class="ds-search-filters__pill"
            data-filter="por-verificar" type="button">
        Por Verificar (80)
    </button>
    <button class="ds-search-filters__pill"
            data-filter="verificados" type="button">
        Verificados (70)
    </button>
</div>
```

| Clase | Descripción |
|-------|-------------|
| `.ds-search-filters` | Flex row con wrap |
| `.ds-search-filters__pill` | Botón pill con borde primary-dark, height 26px, font 0.75rem |
| `.ds-search-filters__pill--active` | Fondo primary-dark, texto blanco |

#### 5.8.4 Search Dropdown (Suggestions)

Aparece debajo del search bar como popup absoluto. Animación `ds-search-dropdown-in` (120ms).

| Clase | Descripción |
|-------|-------------|
| `.ds-search-dropdown` | Panel absoluto, z-index 200, border, radius, shadow |
| `.ds-search-dropdown__header` | Barra con label "Resultados sugeridos" y hint de teclado |
| `.ds-search-dropdown__label` | Texto uppercase 0.625rem |
| `.ds-search-dropdown__hint` | Hint con `<kbd>` estilizado |
| `.ds-search-dropdown__list` | Lista de resultados, max-height 320px scroll |
| `.ds-search-dropdown__empty` | Estado vacío centrado con icono |

#### 5.8.5 Search Result (item del dropdown)

```html
<li class="ds-search-result" data-id="123">
    <div class="ds-search-result__avatar ds-search-result__avatar--person">JP</div>
    <div class="ds-search-result__info">
        <div class="ds-search-result__name">Juan Pérez</div>
        <div class="ds-search-result__meta">0801199912345 · juan@email.com</div>
    </div>
    <button class="ds-search-result__add">
        <span class="material-symbols-outlined">add</span>
    </button>
</li>
```

| Clase | Descripción |
|-------|-------------|
| `.ds-search-result` | Fila flex con gap, hover background, cursor pointer |
| `.ds-search-result--focused` | Resaltado por navegación con teclado |
| `.ds-search-result__avatar` | Círculo 36px con iniciales |
| `.ds-search-result__avatar--person` | Fondo naranja (#ffedd5), texto marrón |
| `.ds-search-result__avatar--company` | Fondo teal (#ccfbf1), texto teal oscuro |
| `.ds-search-result__name` | Nombre del cliente, 0.8125rem, semibold |
| `.ds-search-result__meta` | Identificación + email, 0.6875rem, secondary |
| `.ds-search-result__loaded-badge` | Badge "YA CARGADO" naranja outline |
| `.ds-search-result__add` | Botón circular "+" verde oscuro |

#### 5.8.6 Client List (aside panel)

Panel lateral que muestra la lista completa de clientes cargados.

```html
<aside class="ds-client-list">
    <div class="ds-client-list__header">
        <div class="ds-client-list__meta">
            <div class="ds-client-list__meta-left">
                <span class="ds-client-list__title">Resultados</span>
                <span class="ds-client-list__count-badge">25</span>
            </div>
            <span class="ds-client-list__pager">Mostrando 25 de 150 resultados</span>
        </div>
        <div class="ds-client-list__search">
            <img src="~/icons/search.svg" class="ds-icon ds-client-list__search-icon" />
            <input class="ds-client-list__search-input" placeholder="Filtrar..." />
        </div>
    </div>
    <div class="ds-client-list__body">
        <!-- articles de clientes renderizados por JS -->
    </div>
</aside>
```

| Clase | Descripción |
|-------|-------------|
| `.ds-client-list` | Flex column, border, radius, shadow, height 100% |
| `.ds-client-list__header` | Sticky header con meta info + búsqueda interna |
| `.ds-client-list__title` | Título "Resultados", 0.75rem semibold |
| `.ds-client-list__count-badge` | Badge numérico con fondo primary 10%, 0.625rem bold |
| `.ds-client-list__pager` | Texto "Mostrando X de Y", 0.625rem muted |
| `.ds-client-list__search` | Buscador compacto interno |
| `.ds-client-list__body` | Scroll area, flex: 1 |
| `.ds-client-list__loading-msg` | Texto "Preparando lista..." centrado |

#### 5.8.7 Client Article (row en el aside)

```html
<article class="ds-client-article" data-id="123">
    <div class="ds-client-article__avatar-wrap">
        <div class="ds-client-article__avatar ds-client-article__avatar--person">JP</div>
    </div>
    <div class="ds-client-article__info">
        <div class="ds-client-article__top">
            <span class="ds-client-article__name">Juan Pérez</span>
            <span class="ds-client-article__id">0801199912345</span>
        </div>
        <div class="ds-client-article__icons">
            <span class="material-symbols-outlined ds-ci ds-ci--verified">phone</span>
            <span class="material-symbols-outlined ds-ci ds-ci--pending">mail</span>
            <span class="material-symbols-outlined ds-ci ds-ci--error">location_on</span>
        </div>
    </div>
</article>
```

| Clase | Descripción |
|-------|-------------|
| `.ds-client-article` | Flex row, border-left 4px transparent, hover/active states |
| `.ds-client-article--active` | Fondo selected, border-left primary-dark |
| `.ds-client-article--skeleton` | Skeleton loading (pointer-events none) |
| `.ds-client-article__avatar` | Círculo 36px (mismas variantes que search result) |
| `.ds-client-article__avatar--person` | Naranja |
| `.ds-client-article__avatar--company` | Teal |
| `.ds-client-article__avatar--photo` | Para foto real |
| `.ds-client-article__avatar-ring` | Anillo externo decorativo (para fotos) |
| `.ds-client-article__name` | Nombre, 0.8125rem bold |
| `.ds-client-article__id` | Identificación, 0.625rem bold |
| `.ds-client-article__icons` | Fila de iconos de contacto |
| `.ds-client-article__actions` | Botones hover (editar, ver detalle) — opacity 0 → 1 en hover |
| `.ds-client-article__action-btn` | Botón circular 28px con fondo gris |

#### 5.8.8 Contact Icon `.ds-ci`

Componente micro para mostrar el estado de verificación de un tipo de contacto (teléfono, email, dirección).

```html
<span class="material-symbols-outlined ds-ci ds-ci--verified"
      title="Teléfono" aria-label="Teléfono - verificado" role="img">phone</span>
```

| Clase | Color | Significado |
|-------|-------|-------------|
| `.ds-ci--verified` | `var(--color-success)` verde | Contacto verificado |
| `.ds-ci--pending` | `var(--color-text-muted)` gris | Pendiente de verificación |
| `.ds-ci--error` | `var(--color-danger)` rojo | Con error o rechazado |

---

## 6. Módulos JS del Design System

Todos los módulos JS viven en `wwwroot/js/ds/`. El core (`ds-core.js`) crea el namespace `window.DS` y los demás módulos se registran en `DS.modules` para auto-inicialización.

### 6.1 DS.events — EventBus

Bus de eventos desacoplado para comunicación entre módulos sin dependencias directas.

| Método | Descripción | Retorno |
|--------|-------------|---------|
| `DS.events.on(event, fn)` | Escuchar evento | Función `unsub` para desuscribirse |
| `DS.events.off(event, fn)` | Dejar de escuchar | — |
| `DS.events.emit(event, data)` | Emitir evento a todos los listeners | — |
| `DS.events.once(event, fn)` | Escuchar solo la primera vez | — |

**Eventos conocidos del sistema:**

| Evento | Datos | Emitido por |
|--------|-------|-------------|
| `search:filterChanged` | `{ filter }` | ds-search.js (al cambiar pill) |
| `cliente:seleccionado` | `{ id }` | ds-search.js (al seleccionar de lista o dropdown) |
| `cliente:agregar` | `{ id }` | ds-search.js (botón "+" en dropdown) |
| `cliente:creado` | `{ id, ... }` | Páginas (después de crear) |
| `cliente:editado` | `{ id, ... }` | Páginas (después de editar) |
| `cliente:eliminado` | `{ id }` | Páginas (después de eliminar) |
| `state:{key}` | valor nuevo | DS.state (al cambiar un valor) |
| `api:request` | `{ url, options }` | DS.api (antes de cada request) |
| `api:response` | `{ url, response }` | DS.api (respuesta exitosa) |
| `api:error` | `{ url, error }` | DS.api (error en request) |

```js
// Ejemplo: recargar lista cuando otro módulo crea un cliente
DS.events.on('cliente:creado', ({ id }) => {
    console.log('Nuevo cliente:', id);
    actualizarDashboard();
});
```

### 6.2 DS.utils — Utilidades

| Método | Descripción |
|--------|-------------|
| `DS.utils.debounce(fn, delay=300)` | Retrasa ejecución mientras siguen llegando llamadas |
| `DS.utils.throttle(fn, limit=200)` | Limita a 1 ejecución por intervalo |
| `DS.utils.formatCurrency(value, currency='CLP', locale='es-CL')` | Formato moneda → `$99.900` |
| `DS.utils.formatNumber(value, locale='es-CL')` | Formato número → `1.234.567` |
| `DS.utils.formatDate(dateStr, opts?)` | Formato fecha → `23/02/2026` |
| `DS.utils.truncate(str, max=40)` | Trunca con ellipsis |
| `DS.utils.escapeHtml(str)` | Escapa `& < > " '` para prevenir XSS |
| `DS.utils.qs(selector, context?)` | `querySelector` con warning si no encuentra |
| `DS.utils.qsAll(selector, context?)` | `querySelectorAll` como Array |
| `DS.utils.getAntiForgeryToken()` | Lee el token CSRF de ASP.NET del DOM |
| `DS.utils.toQueryString(params)` | Objeto → `'key=value&key2=value2'` (filtra nulls) |
| `DS.utils.sleep(ms)` | Promise que resuelve después de ms |

### 6.3 DS.state — Estado reactivo

Almacén key-value simple que emite eventos cuando un valor cambia.

| Método | Descripción |
|--------|-------------|
| `DS.state.set(key, value)` | Establece valor. Si cambió, emite `state:{key}` |
| `DS.state.get(key)` | Lee valor actual |
| `DS.state.getAll()` | Copia del store completo |

```js
DS.state.set('filtroActivo', 'pendiente');
DS.events.on('state:filtroActivo', (val) => {
    console.log('Filtro cambió a:', val);
});
```

### 6.4 DS.modules — Registro de módulos

Sistema de auto-inicialización. Cada módulo DS se registra y se ejecuta automáticamente en `DOMContentLoaded`.

| Método | Descripción |
|--------|-------------|
| `DS.modules.register(name, initFn)` | Registra función de inicialización |
| `DS.modules.initAll()` | Ejecuta todas las funciones registradas (auto-llamado) |

```js
// Dentro de un módulo DS:
DS.modules.register('miModulo', () => {
    // Código de inicialización
});
```

### 6.5 DS.notify — Notificaciones (`ds-notifications.js`)

Registrado como módulo 'notifications'. Crea el contenedor `#ds-toast-container` si no existe.

#### Toasts

| Método | Descripción |
|--------|-------------|
| `DS.notify.success(msg, opts?)` | Toast verde ✓ |
| `DS.notify.error(msg, opts?)` | Toast rojo ✗ |
| `DS.notify.warning(msg, opts?)` | Toast amarillo ⚠ |
| `DS.notify.info(msg, opts?)` | Toast cyan ℹ |

**Opciones:** `{ duration: 4000 }` — duración en ms. `0` = no auto-cerrar.

#### Confirmación

```js
DS.notify.confirm('¿Eliminar cliente Juan Pérez?', {
    title: 'Confirmar eliminación',  // opcional
    type: 'danger',                  // 'danger' | 'warning' | 'primary'
    confirmText: 'Eliminar',        // texto del botón confirmar
    onConfirm: () => { /* acción */ },
    onCancel: () => { /* opcional */ }
});
```

Crea un modal de confirmación. Soporta cierre con `Escape`.

### 6.6 DS.api — HTTP (`ds-api.js`)

Registrado como módulo 'api'. Wrapper sobre `fetch()` con manejo automático de errores.

| Método | Descripción |
|--------|-------------|
| `DS.api.get(url, opts?)` | GET request |
| `DS.api.post(url, body?, opts?)` | POST con JSON body |
| `DS.api.put(url, body?, opts?)` | PUT con JSON body |
| `DS.api.patch(url, body?, opts?)` | PATCH con JSON body |
| `DS.api.delete(url, body?, opts?)` | DELETE |

**Retorno:** `Promise<{ ok: boolean, data: any, error: string, status: number }>`

**Comportamiento automático:**
- Incluye `Content-Type: application/json`
- Incluye anti-forgery token de ASP.NET en el header
- 401 → redirige a página de login automáticamente
- Emite eventos: `api:request`, `api:response`, `api:error`
- Opción `silent: true` suprime notificaciones de error

```js
const { ok, data } = await DS.api.get('/api/clientes');
if (ok) renderTabla(data);

const result = await DS.api.post('/api/clientes', { nombre: 'Juan' });
if (result.ok) DS.notify.success('Cliente creado');
```

### 6.7 DS.modal — Modales (`ds-modal.js`)

Registrado como módulo 'modal'. Gestiona un stack de modales con bloqueo de scroll y focus.

| Método | Descripción |
|--------|-------------|
| `DS.modal.open(id)` | Abre modal declarativo por su DOM id |
| `DS.modal.close()` | Cierra el modal superior del stack (con animación de salida) |
| `DS.modal.closeAll()` | Cierra todos los modales |
| `DS.modal.create(opts)` | Crea modal dinámico programáticamente |

**Opciones de `create`:**

```js
DS.modal.create({
    title: 'Editar cliente',
    size: 'lg',                    // 'sm' | 'lg' | 'xl' | (vacío=md)
    body: '<form>...</form>',      // HTML string o Element
    closable: true,                // si puede cerrarse con X/Escape
    footer: [
        { text: 'Cancelar', variant: 'ghost', action: 'cancel' },
        { text: 'Guardar',  variant: 'primary', action: 'confirm', id: 'btn-guardar' }
    ],
    onConfirm: async (backdrop) => { /* ... */ },
    onCancel: () => { /* ... */ }
});
```

**Comportamiento:** body overflow lock, Escape key handler, focus management.

### 6.8 DS.form — Formularios (`ds-form.js`)

Registrado como módulo 'form'.

| Método | Descripción |
|--------|-------------|
| `DS.form.validate(selector)` | Validación HTML5 (required, email, minlength, pattern). Agrega `.is-invalid`/`.is-valid` y muestra `.ds-field-error` |
| `DS.form.serialize(selector)` | FormData a objeto plano (soporta multiple values) |
| `DS.form.submitAjax(selector, opts)` | validate → serialize → DS.api call → loading state |
| `DS.form.fill(selector, data)` | Rellena formulario desde objeto (soporta checkbox/radio) |
| `DS.form.reset(selector)` | Reset HTML + limpia clases de validación |
| `DS.form.setLoading(selector, bool)` | Deshabilita controles + `.is-loading` en submit button |

**Opciones de `submitAjax`:**

```js
const result = await DS.form.submitAjax('#form-cliente', {
    url: '/api/clientes',         // si no lo tiene el form.action
    method: 'POST',               // si no lo tiene el form.method
    onSuccess: (data) => {
        DS.notify.success('Guardado');
        DS.modal.close();
        DS.events.emit('cliente:creado', data);
    }
});
```

### 6.9 DS.search — Búsqueda global (`ds-search.js`)

Módulo IIFE (no usa `DS.modules.register`). Se auto-inicializa en `DOMContentLoaded`.

**API pública:**

| Método | Descripción |
|--------|-------------|
| `DS.search.init()` | Inicializa (auto-llamado). Cachea DOM, bind eventos |
| `DS.search.reload()` | Recarga la lista lateral de clientes |

**Comportamiento interno:**

1. **Búsqueda global:** Input con debounce (300ms) → `DS.api.get('/api/clientes/serach?q=...&estado=...&top=10')` → renderiza dropdown con resultados
2. **Filter pills:** Click en pill → toggle `--active` → re-fetch si hay query activa + filtro local en sidebar
3. **Dropdown:** Navegación con teclado (ArrowUp/Down/Enter/Escape), click outside para cerrar
4. **Client list aside:** Carga inicial via `DS.api.get('/api/clientes')` → renderiza articles con avatars, contacto icons
5. **Búsqueda local en sidebar:** Input con debounce (250ms) filtra articles por nombre/ID (sin API call)
6. **Skeleton loaders:** Se muestran durante carga tanto en dropdown como en sidebar
7. **Contact icons:** Usa `_buildContactIconHTML()` con estados del DTO: `estadoTelefono`, `estadoEmail`, `estadoDireccion`

**Eventos emitidos:**
- `search:filterChanged` — `{ filter: 'todos'|'por-verificar'|'verificados' }`
- `cliente:seleccionado` — `{ id }` (click en resultado de dropdown o article de sidebar)
- `cliente:agregar` — `{ id }` (click en botón "+" del dropdown)

**Eventos escuchados:**
- `cliente:creado` → recarga lista lateral
- `cliente:editado` → recarga lista lateral
- `cliente:eliminado` → recarga lista lateral

---

## 7. Componentes Razor (Partials)

Todos viven en `Pages/Shared/Components/`. Se incluyen con `<partial>` o `Html.PartialAsync`.

### 7.1 `_DsPageHeader.cshtml`

Encabezado estándar de página con título, subtítulo y slot para acciones.

**ViewData:**

| Key | Tipo | Default | Descripción |
|-----|------|---------|-------------|
| `_DsPageHeader_Title` | string | `""` | Título principal |
| `_DsPageHeader_Subtitle` | string | `""` | Subtítulo descriptivo |

**RenderSection:** `PageActions` (slot derecho para botones).

```html
@await Html.PartialAsync("Components/_DsPageHeader", new ViewDataDictionary(ViewData) {
    ["_DsPageHeader_Title"] = "Clientes",
    ["_DsPageHeader_Subtitle"] = "Gestión de contactabilidad"
})
```

### 7.2 `_DsBadge.cshtml`

Badge / pill con variante de estado.

**ViewData:**

| Key | Tipo | Default | Descripción |
|-----|------|---------|-------------|
| `_DsBadge_Text` | string | `""` | Texto del badge |
| `_DsBadge_Variant` | string | `"neutral"` | success, warning, danger, info, alert, primary, neutral |
| `_DsBadge_Dot` | bool | `false` | Muestra punto de color |
| `_DsBadge_Size` | string | `""` | sm, lg, o vacío (normal) |

### 7.3 `_DsCard.cshtml`

Card reutilizable con header y body.

**ViewData:**

| Key | Tipo | Default | Descripción |
|-----|------|---------|-------------|
| `_DsCard_Title` | string | `""` | Título del card |
| `_DsCard_Subtitle` | string | `""` | Subtítulo |
| `_DsCard_Hoverable` | bool | `false` | Efecto hover elevado |
| `_DsCard_Class` | string | `""` | Clases CSS extra |

**RenderSection:** `CardActions` (acciones en el header).
**RenderBody:** Contenido del card body.

### 7.4 `_DsModal.cshtml`

Modal declarativo. Para modales complejos con contenido, usar directamente las clases CSS.

**ViewData:**

| Key | Tipo | Default | Descripción |
|-----|------|---------|-------------|
| `_DsModal_Id` | string | `""` | ID del modal (requerido para `DS.modal.open()`) |
| `_DsModal_Title` | string | `""` | Título del modal |
| `_DsModal_Size` | string | `""` | sm, lg, xl, o vacío (md) |

Genera un modal con footer default (Cancelar + Confirmar). Body e ID del confirm button: `{id}-body`, `{id}-confirm`.

### 7.5 `_DsClientSearch.cshtml`

Barra de búsqueda global con dropdown de sugerencias y pills de filtro.

**ViewData:**

| Key | Tipo | Default | Descripción |
|-----|------|---------|-------------|
| `SearchFilter` | string | `"todos"` | Filtro activo inicial |
| `TotalTodos` | int | `0` | Total de clientes |
| `TotalPorVerif` | int | `0` | Total por verificar |
| `TotalVerif` | int | `0` | Total verificados |
| `SearchPlaceholder` | string | `"Búsqueda Global..."` | Placeholder del input |

**Requiere:** `ds-search.js`, `_search.css`.

**IDs internos fijos:**
- `ds-search-widget`, `ds-search-input`, `ds-search-clear`
- `ds-search-dropdown`, `ds-search-results`, `ds-search-empty`
- Pills con `data-filter`: `todos`, `por-verificar`, `verificados`

### 7.6 `_DsClientList.cshtml`

Panel lateral (aside) de lista de clientes con búsqueda interna y skeleton loading.

**ViewData:**

| Key | Tipo | Default | Descripción |
|-----|------|---------|-------------|
| `ClientListTitle` | string | `"Resultados"` | Título del panel |
| `ClientListCount` | int | `0` | Conteo inicial visible |
| `ClientListTotal` | int | `0` | Total para "Mostrando X de Y" |
| `ClientListSearch` | string | `"Filtrar por nombre o ID..."` | Placeholder búsqueda interna |

**Requiere:** `ds-search.js`, `_search.css`.

**IDs internos fijos:**
- `ds-client-list-count`, `ds-client-list-pager`
- `ds-client-list-search`, `ds-client-list-body`

**Estado inicial:** Muestra 6 skeleton rows + mensaje "Preparando lista..." hasta que JS carga datos reales.

---

## 8. Sistema de iconos

El proyecto usa **tres fuentes de iconos**:

### 8.1 Material Symbols Outlined (principal)

Iconos de texto de Google Fonts. Se usan con la clase `material-symbols-outlined`.

```html
<span class="material-symbols-outlined">add</span>
<span class="material-symbols-outlined">edit</span>
<span class="material-symbols-outlined">delete</span>
<span class="material-symbols-outlined">search</span>
<span class="material-symbols-outlined">phone</span>
<span class="material-symbols-outlined">mail</span>
<span class="material-symbols-outlined">location_on</span>
<span class="material-symbols-outlined">close</span>
<span class="material-symbols-outlined">hub</span>              <!-- Logo navbar -->
```

### 8.2 Material Icons (legacy)

Clase `material-icons`. Algunos componentes legacy todavía los usan.

### 8.3 SVG custom (proyecto)

19 iconos SVG en `wwwroot/icons/`. Se cargan como `<img>` con clase `ds-icon`.

```html
<img src="~/icons/search.svg" class="ds-icon" aria-hidden="true" alt="" />
<img src="~/icons/document-upload.svg" class="ds-icon" aria-hidden="true" alt="" />
<img src="~/icons/verify-check.svg" class="ds-icon contact-verify-icon" aria-hidden="true" alt="" />
```

**Modificadores de tamaño:**

| Clase | Efecto |
|-------|--------|
| `.ds-icon` | Tamaño base (depende del contexto) |
| `.ds-icon--sm` | Versión pequeña |

**Inventario de SVGs:**

| Archivo | Uso |
|---------|-----|
| `search.svg` | Lupa de búsqueda (barra global, list search) |
| `document-upload.svg` | Subida de archivos (lote, dropzone) |
| `edit.svg` | Editar contacto |
| `save.svg` | Guardar |
| `verify-check.svg` | Check de verificación (contactos) |
| `note-remove.svg` / `note-remove-active.svg` | Eliminar nota |
| `error-info.svg` | Información de error |
| `article-open.svg` | Abrir artículo/detalle |
| `section-phone.svg` / `row-phone.svg` / `row-phone2.svg` | Iconos de teléfono (sección/fila) |
| `section-email.svg` / `row-email.svg` | Iconos de email (sección/fila) |
| `section-location.svg` / `row-location.svg` | Iconos de ubicación (sección/fila) |
| `contact-phone.svg` / `contact-email.svg` / `contact-location.svg` | Iconos de tipo de contacto |

---

## 9. Integración con Tailwind CSS

Tailwind CSS coexiste con Bootstrap y el DS custom. Se usa **únicamente para componentes específicos** (como el formulario de carga masiva), NO para reemplazar el Design System.

### Configuración (`tailwind.config.js`)

```js
module.exports = {
    prefix: 'tw-',          // TODAS las clases llevan prefijo tw-
    corePlugins: {
        preflight: false     // NO resetea estilos base (Bootstrap/DS los manejan)
    },
    content: [
        './Pages/**/*.cshtml',
        './Views/**/*.cshtml',
        './Areas/**/*.cshtml',
        './wwwroot/js/**/*.js'
    ],
    theme: {
        extend: {
            colors: {
                primary: {
                    dark:    '#295136',
                    DEFAULT: '#446942',
                    light:   '#7F9787',
                    pale:    '#DBE4DD',
                    surface: '#f5f7f5',
                },
                accent:  '#CB801B',
                // ... más colores que espejean los tokens DS
            }
        }
    }
}
```

### Cuándo usar Tailwind vs DS

| Situación | Usar |
|-----------|------|
| Botón, card, badge, tabla, formulario, modal | **Clases DS** (`.ds-btn`, `.ds-card`, etc.) |
| Layout de un componente aislado (ej: dropzone de archivo) | **Tailwind** (`tw-flex`, `tw-p-4`, etc.) |
| Colores, sombras, espaciado | **Tokens DS** (`var(--color-primary)`, `var(--spacing-4)`) |
| Utility rápida que no existe en DS ni Bootstrap | **Tailwind** con prefijo `tw-` |

### Ejemplo real: FileUpload con Tailwind

```html
<div class="tw-border-2 tw-border-dashed tw-border-form-border tw-rounded-xl tw-p-6 tw-text-center">
    <p class="tw-text-sm tw-text-body-muted">Arrastra tu archivo aquí</p>
</div>
```

---

## 10. Patrón recomendado por pantalla nueva

```js
// wwwroot/js/mipagina.js

(function () {
    'use strict';

    // ─── Estado local de la página ────────────────────────
    let clientes = [];
    let paginaActual = 1;

    // ─── Inicializar ──────────────────────────────────────
    async function init() {
        await cargarClientes();
        bindEventos();
    }

    // ─── Carga de datos ───────────────────────────────────
    async function cargarClientes() {
        const { ok, data } = await DS.api.get('/api/clientes');
        if (!ok) return;
        clientes = data;
        renderTabla(clientes);
    }

    // ─── Render ───────────────────────────────────────────
    function renderTabla(lista) {
        const tbody = DS.utils.qs('#tbody-clientes');
        if (!lista.length) {
            DS.utils.qs('#row-empty').style.display = '';
            return;
        }
        tbody.innerHTML = lista.map(c => `
            <tr>
                <td>${DS.utils.escapeHtml(c.nombre)}</td>
                <td>
                    <span class="ds-badge ds-badge--${c.activo ? 'success' : 'neutral'}
                                 ds-badge--dot">
                        ${c.activo ? 'Activo' : 'Inactivo'}
                    </span>
                </td>
                <td class="ds-table__col--actions">
                    <button class="ds-btn ds-btn--ghost ds-btn--icon ds-btn--sm"
                            data-action="editar" data-id="${c.id}">
                        <span class="material-symbols-outlined">edit</span>
                    </button>
                </td>
            </tr>
        `).join('');
    }

    // ─── Eventos ──────────────────────────────────────────
    function bindEventos() {
        // Búsqueda con debounce
        DS.utils.qs('#input-buscar')?.addEventListener('input',
            DS.utils.debounce(e => {
                const q = e.target.value.toLowerCase();
                renderTabla(clientes.filter(c =>
                    c.nombre.toLowerCase().includes(q)
                ));
            }, 300)
        );

        // Delegación de clic en tabla
        DS.utils.qs('#tbody-clientes')?.addEventListener('click', e => {
            const btn = e.target.closest('[data-action]');
            if (!btn) return;
            const id = btn.dataset.id;
            if (btn.dataset.action === 'editar') abrirEditar(id);
            if (btn.dataset.action === 'eliminar') confirmarEliminar(id);
        });

        // Escuchar eventos de otros módulos
        DS.events.on('cliente:creado', () => cargarClientes());
        DS.events.on('cliente:editado', () => cargarClientes());
    }

    // ─── Acciones ─────────────────────────────────────────
    async function confirmarEliminar(id) {
        DS.notify.confirm('¿Eliminar este cliente?', {
            type: 'danger',
            onConfirm: async () => {
                const { ok } = await DS.api.delete(`/api/clientes/${id}`);
                if (ok) {
                    DS.notify.success('Cliente eliminado');
                    DS.events.emit('cliente:eliminado', { id });
                    cargarClientes();
                }
            }
        });
    }

    // ─── Start ────────────────────────────────────────────
    document.addEventListener('DOMContentLoaded', init);

})();
```

### Estructura Razor sugerida

```html
@page
@model MiPaginaModel
@{ ViewData["Title"] = "Mi Página"; }

@section Styles {
    <link rel="stylesheet" href="~/css/mipagina.css" />
}

<!-- Page header -->
<partial name="Shared/Components/_DsPageHeader"
         view-data='@(new ViewDataDictionary(ViewData) {
             ["_DsPageHeader_Title"] = "Mi Página",
             ["_DsPageHeader_Subtitle"] = "Descripción de la página"
         })' />

<!-- Si necesita búsqueda global + lista lateral -->
<partial name="Shared/Components/_DsClientSearch" />
<partial name="Shared/Components/_DsClientList" />

<!-- Contenido principal -->
<div class="ds-card">
    <div class="ds-card__body">
        <!-- tabla, formulario, etc. -->
    </div>
</div>

@section Scripts {
    <script src="~/js/mipagina.js"></script>
}
```

---

## 11. Tokens de referencia rápida

| Token | Valor | Categoría |
|-------|-------|-----------|
| `--color-primary` | `#446942` | Color |
| `--color-primary-dark` | `#295136` | Color |
| `--color-primary-light` | `#7F9787` | Color |
| `--color-success` | `#28a745` | Estado |
| `--color-danger` | `#dc3545` | Estado |
| `--color-warning` | `#ffc107` | Estado |
| `--color-info` | `#0dcaf0` | Estado |
| `--color-alert` | `#CB801B` | Estado |
| `--color-bg-page` | `#f5f7f5` | Fondo |
| `--color-bg-surface` | `#ffffff` | Fondo |
| `--color-text-primary` | `#295136` | Texto |
| `--color-text-secondary` | `rgba(41,81,54, 0.70)` | Texto |
| `--color-text-muted` | `rgba(41,81,54, 0.50)` | Texto |
| `--color-border` | `rgba(41,81,54, 0.12)` | Borde |
| `--font-size-xs` | `0.75rem` (12px) | Tipografía |
| `--font-size-sm` | `0.875rem` (14px) | Tipografía |
| `--font-size-base` | `1rem` (16px) | Tipografía |
| `--font-weight-semibold` | `600` | Tipografía |
| `--font-weight-bold` | `700` | Tipografía |
| `--radius-md` | `0.5rem` | Radius |
| `--radius-lg` | `0.75rem` | Radius |
| `--radius-xl` | `1rem` | Radius |
| `--radius-full` | `9999px` | Radius |
| `--spacing-2` | `0.5rem` (8px) | Espaciado |
| `--spacing-4` | `1rem` (16px) | Espaciado |
| `--spacing-6` | `1.5rem` (24px) | Espaciado |
| `--spacing-8` | `2rem` (32px) | Espaciado |
| `--shadow-card` | `0 2px 12px rgba(41,81,54, .04)` | Sombra |
| `--shadow-hover` | `0 8px 24px rgba(41,81,54, .12)` | Sombra |
| `--shadow-modal` | `0 20px 60px rgba(41,81,54, .20)` | Sombra |
| `--transition-fast` | `150ms ease` | Transición |
| `--transition-base` | `250ms ease` | Transición |
| `--navbar-height` | `64px` | Layout |
| `--aside-width` | `426px` | Layout |

---

## 12. Notas de migración y legado

### Bootstrap → DS

El proyecto está en proceso de migración gradual de Bootstrap al Design System custom. Bootstrap **sigue cargado** como base y algunos componentes legacy dependen de él.

**Archivo `bootstrap-overrides.css`** define clases puente:
- `.btn-forest-green` — Botón verde que usa tokens DS
- `.text-forest-green` — Texto con color primary-dark
- `.bg-forest-green` — Fondo primary-dark
- `.border-forest-green` — Borde primary-dark
- `.bg-light-subtle`, `.border-subtle` — Fondos y bordes suaves

**Regla:** Para nuevos componentes, usar siempre clases `.ds-*`. No crear nuevos componentes con clases Bootstrap.

### Aliases legacy en `variables.css`

Estas variables existen para compatibilidad con CSS de páginas antiguas. **No usar en código nuevo:**

| Variable legacy | Apunta a |
|---|---|
| `--forest-green` | `var(--color-primary-dark)` |
| `--background-light` | `var(--color-bg-light)` |
| `--verde-musgo` | `var(--color-primary-light)` |
| `--ocre-intenso` | `var(--color-alert)` |
| `--ocre-hover` | `#A86915` |
| `--alert-ochre` | `var(--color-alert)` |
| `--success-green` | `var(--color-success)` |
| `--danger-red` | `var(--color-danger)` |
| `--modal-bg` | `var(--color-bg-subtle)` |
| `--bs-body-font-family` | `var(--font-family-base)` |

**Plan:** Eliminar estos aliases cuando cada página esté completamente migrada al DS.

### Coexistencia de frameworks CSS

```
Bootstrap (base)  →  Tailwind (tw-*)  →  Tokens (variables.css)  →  DS (ds/ds.css)  →  Overrides  →  Page CSS
```

La especificidad aumenta de izquierda a derecha. El DS siempre tiene prioridad sobre Bootstrap.
Tailwind usa prefijo `tw-` para evitar colisiones.
