# Plan de Trabajo — Migración Bootstrap → Design System

**Estado:** Pendiente  
**Prioridad:** Alta  
**Impacto:** Elimina dependencia externa (CDN), reduce peso de página, unifica estilos bajo el DS

---

## Contexto

Bootstrap 5.3.3 se carga desde CDN en `_Layout.cshtml` y se usa en ~126 puntos en las páginas Razor. El DS ya cubre el 90% de los casos de uso de Bootstrap. La migración es conservadora: Tailwind (`tw-`) cubre los utilitarios de layout; el DS cubre componentes.

### Archivos afectados

| Archivo | Tipo de uso Bootstrap |
|---|---|
| `Pages/Shared/_Layout.cshtml` | Navbar, CDN links, scripts |
| `Pages/Users.cshtml` | Grid, form controls, utilities |
| `Pages/BulkUpload.cshtml` | Grid, form controls |
| `Pages/Shared/Components/FileUpload/_SectionLightGreenFormArea.cshtml` | Form controls |
| `wwwroot/css/bootstrap-overrides.css` | Sobreescrituras (eliminar al final) |

### Conteo de clases a migrar

| Clase Bootstrap | Ocurrencias | Reemplazo |
|---|---|---|
| `d-flex` | 14 | `tw-flex` |
| `form-control` | 11 | `.ds-input` |
| `align-items-center` | 10 | `tw-items-center` |
| `d-block` | 8 | `tw-block` |
| `fw-bold` | 7 | `tw-font-bold` |
| `d-none` | 6 | `tw-hidden` |
| `navbar-*` | 4 | `.navbar-custom` (ya en `layout.css`) |
| `col-md-6` | 4 | `tw-w-1/2` / CSS Grid |
| `btn btn-*` | 4 | `.ds-btn .ds-btn--*` |
| `nav-link` | 3 | Custom en `layout.css` |
| `justify-content-between` | 3 | `tw-justify-between` |
| `text-muted` | 2 | `tw-text-gray-500` o `var(--color-text-muted)` |
| `modal fade` | 2 | `.ds-modal` |
| `form-select` | 2 | `.ds-select` |
| `text-center` | 1 | `tw-text-center` |

---

## Tabla de Equivalencias

### Layout y Display

| Bootstrap | DS / Tailwind |
|---|---|
| `d-flex` | `tw-flex` |
| `d-none` | `tw-hidden` |
| `d-block` | `tw-block` |
| `d-grid` | `tw-grid` |
| `d-inline-flex` | `tw-inline-flex` |
| `align-items-center` | `tw-items-center` |
| `align-items-start` | `tw-items-start` |
| `justify-content-between` | `tw-justify-between` |
| `justify-content-center` | `tw-justify-center` |
| `justify-content-end` | `tw-justify-end` |
| `gap-1` … `gap-4` | `tw-gap-1` … `tw-gap-4` |
| `col-md-6` | `tw-w-1/2` |
| `col-md-4` | `tw-w-1/3` |
| `col-md-3` | `tw-w-1/4` |
| `container` | `.container-custom` (ya en `bootstrap-overrides.css`, migrar a DS) |
| `row` + `col-*` | `tw-grid tw-grid-cols-*` |

### Tipografía y Color

| Bootstrap | DS / Tailwind |
|---|---|
| `fw-bold` | `tw-font-bold` |
| `fw-semibold` | `tw-font-semibold` |
| `fw-normal` | `tw-font-normal` |
| `fs-1` … `fs-6` | `tw-text-4xl` … `tw-text-sm` (mapear según tamaño) |
| `text-muted` | `tw-text-gray-500` o `style="color: var(--color-text-muted)"` |
| `text-center` | `tw-text-center` |
| `text-end` | `tw-text-right` |
| `text-start` | `tw-text-left` |
| `text-forest-green` | `style="color: var(--color-primary)"` |
| `bg-forest-green` | `style="background: var(--color-primary)"` |

### Componentes

| Bootstrap | DS |
|---|---|
| `btn btn-primary` | `ds-btn ds-btn--primary` |
| `btn btn-secondary` | `ds-btn ds-btn--secondary` |
| `btn btn-danger` | `ds-btn ds-btn--danger` |
| `btn btn-success` | `ds-btn ds-btn--success` |
| `btn btn-outline-*` | `ds-btn ds-btn--ghost` |
| `btn btn-sm` | `ds-btn ds-btn--sm` |
| `btn btn-lg` | `ds-btn ds-btn--lg` |
| `btn-forest-green` | `ds-btn ds-btn--primary` (eliminar clase custom) |
| `form-control` | `ds-input` |
| `form-select` | `ds-select` |
| `form-label` | `ds-form-group label` (estructura DS) |
| `input-group` | Reemplazar con `ds-form-group` |
| `badge` + variante | `ds-badge ds-badge--*` |
| `card` | `ds-card` |
| `modal fade` + estructura | `.ds-modal` + `DS.modal.open()` |
| `nav-link` | Custom en `layout.css` |

### Espaciado (solo cuando no hay alternativa DS)

Los utilitarios de spacing de Bootstrap (`mb-2`, `mt-3`, etc.) son aceptables temporalmente en Tailwind si no existe componente DS para esos casos. Preferir siempre CSS variables en hojas de estilo por página.

| Bootstrap | Tailwind |
|---|---|
| `mb-1` … `mb-4` | `tw-mb-1` … `tw-mb-4` |
| `mt-*` | `tw-mt-*` |
| `me-*` / `ms-*` | `tw-mr-*` / `tw-ml-*` |
| `p-*` / `px-*` / `py-*` | `tw-p-*` / `tw-px-*` / `tw-py-*` |

---

## Proceso de Migración por Fase

### Fase 1 — Formularios (riesgo bajo, alto impacto)

**Archivos:** `Users.cshtml`, `BulkUpload.cshtml`, `_SectionLightGreenFormArea.cshtml`

1. Reemplazar `form-control` → `ds-input`
2. Reemplazar `form-select` → `ds-select`
3. Envolver campos en `<div class="ds-form-group">` con `<label>` y estado de validación
4. Probar: validación HTML5, estados error/success, responsive

### Fase 2 — Botones

**Archivos:** Todos los `.cshtml`

1. Reemplazar `btn btn-primary` / `btn-forest-green` → `ds-btn ds-btn--primary`
2. Reemplazar variantes secundarias, ghost, danger
3. Eliminar `btn-forest-green` de `bootstrap-overrides.css` cuando no quede ninguna referencia

### Fase 3 — Layout y Utilitarios

**Archivos:** Todos los `.cshtml`

1. Reemplazar clases de display (`d-flex`, `d-none`, etc.) → Tailwind equivalentes
2. Reemplazar alineaciones → Tailwind
3. Reemplazar grid Bootstrap (`row`/`col-*`) → `tw-grid tw-grid-cols-*` o CSS Grid en la hoja de página

### Fase 4 — Modales

**Archivos:** Páginas que usan `modal fade`

1. Convertir estructura Bootstrap modal → `.ds-modal` con atributos `data-ds-modal-id`
2. Reemplazar llamadas JS a `new bootstrap.Modal(...)` → `DS.modal.open(id)`
3. Probar: apertura, cierre, foco atrapado, backdrop

### Fase 5 — Navbar

**Archivos:** `_Layout.cshtml`

El navbar ya usa `.navbar-custom` definido en `layout.css`. Revisar qué clases Bootstrap quedan en el markup y reemplazar con clases custom o Tailwind. Esta fase afecta todas las páginas — hacer al final.

### Fase 6 — Limpieza Final

Una vez que ninguna página use clases Bootstrap:

1. Eliminar el link de Bootstrap CSS de `_Layout.cshtml` (CDN + fallback local)
2. Eliminar el script de Bootstrap JS de `_Layout.cshtml`
3. Eliminar `wwwroot/lib/bootstrap/` (librería local de fallback)
4. Eliminar `wwwroot/css/bootstrap-overrides.css`
5. Ejecutar búsqueda global de `bootstrap` en `Pages/` para confirmar que no queda ninguna referencia

---

## Criterios de Aceptación por Fase

Antes de marcar una fase como completada:

- [ ] Sin regresiones visuales en la página migrada (revisar en pantalla de 1440px y 768px)
- [ ] Sin errores en consola del navegador
- [ ] Formularios validan correctamente
- [ ] Ninguna clase Bootstrap activa en DevTools para los elementos migrados

---

## Tabla de Seguimiento

| Fase | Páginas | Estado | Responsable |
|---|---|---|---|
| 1 — Formularios | Users, BulkUpload, FileUpload | Pendiente | — |
| 2 — Botones | Todas | Pendiente | — |
| 3 — Layout | Todas | Pendiente | — |
| 4 — Modales | Con modal fade | Pendiente | — |
| 5 — Navbar | _Layout | Pendiente | — |
| 6 — Limpieza | _Layout, lib/ | Pendiente | — |

---

## Información Faltante

- [ ] Confirmar si hay páginas fuera de `Pages/` que usen clases Bootstrap (búsqueda en `wwwroot/js/`).
- [ ] Definir si Bootstrap JS puede eliminarse ahora (si ningún componente JS de Bootstrap está en uso: tooltips, popovers, offcanvas).
- [ ] Confirmar si `lib/bootstrap/` local puede eliminarse del repositorio tras la migración.
