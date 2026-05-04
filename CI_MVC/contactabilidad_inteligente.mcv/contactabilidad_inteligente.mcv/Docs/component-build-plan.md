# Component Build Plan

> **Última actualización:** 2026-03-09  
> **Sesión:** multi | **Estrategia:** extend tokens

---

## 📋 Inventario de Componentes

### [2] Section - Light Green Background Area for Form Content
- **Status:** ✅ COMPLETADO (2026-03-09)
- **Figma Node:** `116:4291` (Data-Smart-Hub)
- **Ubicación:** `Pages/Shared/Components/FileUpload/_SectionLightGreenFormArea.cshtml`
- **ViewModel:** `Application/Models/SectionLightGreenFormAreaViewModel.cs`
- **JavaScript:** `wwwroot/js/file-upload-component.js`
- **Documentación:** `Docs/components/section-light-green-form-area.md`
- **Tailwind Tokens:** Extendidos en `tailwind.config.js` 
- **Características:**
  - ✅ Zona de arrastre y soltar (drag & drop)
  - ✅ Click para seleccionar archivo
  - ✅ Validación de tipo y tamaño de archivo
  - ✅ Mostrar detalles de archivo cargado
  - ✅ Indicador de registros detectados
  - ✅ Botón descargar plantilla
  - ✅ Accesibilidad ARIA
  - ✅ Responsividad Tailwind

---

## 📌 Ficha Técnica (FASE 1 — congelada)

### [2] Section - Light Green Form Area

> Fuente: Figma `lJSE2H8XeNXlNRFAcJWYzd` · node `116:4291`

**Estructura atómica:**

```
Section-LightGreenFormArea  (Full Width)
├── [1] Upload Drop Zone  (bg-white, border-dashed, 12px radius)
│   ├── Cloud Icon  (24×33px)
│   ├── Title Text  (Inter Bold 14px, #295136)
│   ├── Subtitle Text  (Inter Medium 14px, #334155)
│   └── Download Button  (flex, gap=6px, text=12px)
│
├── [2] File Item Card  (bg-white, 8px radius, shadow-sm)
│   ├── Icon Container  (40×40px, bg=#f0fdf4)
│   ├── File Details  (flexcol, gap=1)
│   │   ├── Filename  (Nimbus Bold 14px, #1b2b21)
│   │   └── File Size  (Nimbus Regular 12px, #9ca3af)
│   └── Success Checkmark  (28×20px, #22c55e)
│
└── [3] Info Alert Box  (bg=#cfe2d6, 6px radius)
    ├── Info Icon  (18×18px)
    └── Message  (Nimbus Regular 14px, #1b2b21)
```

---

### [1] dash_client_Searching (ANTERIOR)

> Fuente: Figma `lJSE2H8XeNXlNRFAcJWYzd` · node `110:3991`

### Estructura atómica

```
dash_client_Searching  (1264 × 1008 px)

## 📌 Ficha Técnica (FASE 1 — congelada)

> Fuente: Figma `lJSE2H8XeNXlNRFAcJWYzd` · node `110:3991`

### Estructura atómica

```
dash_client_Searching  (1264 × 1008 px)
├── Container (flex row, justify-between, gap=auto)
│   ├── Aside  (426 px, flex-col, rounded-8, border 1px rgba(41,81,54,.1), shadow-card)
│   │   ├── Header  (border-bottom, px=12, py=8–9, flex-col, gap=8)
│   │   │   ├── Meta row  (flex, justify-between)
│   │   │   │   ├── "Resultados" label  (Sora SemiBold 12px, #295136)
│   │   │   │   ├── count badge  (bg rgba(41,81,54,.1), px=6 py=2, radius=4)
│   │   │   │   └── pager text  (Sora Regular 10px, rgba(41,81,54,.6))
│   │   │   └── Search input  (bg rgba(240,244,242,.5), border rgba(41,81,54,.1), radius=4, px=9 py=5)
│   │   └── Body  (flex-1, overflow-y, z=1)
│   │       ├── Article (active)  bg=#ebf2ee, border-l=4px #295136
│   │       ├── Article (default) border-l=4px rgba(41,81,54,.05)
│   │       └── Skeleton rows    opacity 0.6 → 0.4 → 0.2
│   └── Detail Panel  (flex-1, right side — scope separado)
│
└── Global Search Card  (overlays top)
    ├── Search Bar  (flex, bg-page, border, radius-xl, px=16 py=12)
    │   ├── search icon  (18px, text-muted)
    │   └── input  (placeholder="Búsqueda Global...", Inter/Sora 14px)
    ├── Filter Pills  (flex, gap=8, mt=8)
    │   ├── Todos (N)        — inactive (white bg, #295136 border)
    │   ├── Por Verificar(N) — active (#295136 bg, white text)
    │   └── Verificados (N)  — inactive
    └── Suggestions Dropdown  (absolute, z=200, bg-white, shadow-hover, radius-lg)
        ├── Header  (uppercase label + kbd hint)
        └── Result rows  (avatar 36px + name 13px + meta 11px + action)
            ├── Variant "YA CARGADO"  — amber ochre badge
            └── Variant default       — + button (#295136 circle, 28px)
```

### Tokens visuales clave

| Propiedad | Valor Figma | Token DS |
| :--- | :--- | :--- |
| Primary dark | `#295136` | `--color-primary-dark` |
| Selected row bg | `#ebf2ee` | `--color-bg-selected` *(nuevo)* |
| Border list | `rgba(41,81,54,0.05)` | `--color-border-list` *(nuevo)* |
| Surface | `#ffffff` | `--color-bg-surface` |
| Search input bg | `rgba(240,244,242,0.5)` | CSS inline (no token) |
| Amber badge | `#CB801B` | `--color-alert` |
| Font heading | Sora SemiBold/Bold | `font-family: 'Sora'` |
| Font pill | Inter Medium 12px | `font-family: 'Inter'` |
| Card shadow | `0 2px 12px rgba(41,81,54,.04)` | `--shadow-card` |
| Hover shadow | `0 8px 24px rgba(41,81,54,.12)` | `--shadow-hover` |
| Pill height | 26px, radius full | `--radius-full` |
| Avatar | 36px circle | `--radius-full` |
| Action btn | 28px circle | `--radius-full` |

### Variantes y estados

| Estado | Descripción |
| :--- | :--- |
| Default row | `border-l: 4px solid rgba(41,81,54,.05)` |
| Active row | `bg: #ebf2ee`, `border-l: 4px #295136` |
| Hover row | acciones visibles (opacity 0→1), fondo leve |
| Skeleton (loading) | Filas con opacity 0.6 → 0.4 → 0.2, bg #e5e7eb |
| Search open | dropdown visible, `aria-expanded=true` |
| Filter active pill | `bg: #295136`, `color: white` |
| Result "ya cargado" | badge amber en lugar de botón `+` |

---

## ✅ Checklist de construcción

### Tokens

- [x] `--color-bg-selected: #ebf2ee` añadido a `variables.css`
- [x] `--color-border-list: rgba(41,81,54,0.05)` añadido a `variables.css`
- [x] `primary.surface`, `bg.selected`, `border.list` añadidos a `tailwind.config.js`

### CSS

- [x] `wwwroot/css/ds/components/_search.css` creado
- [x] `wwwroot/css/ds/ds.css` actualizado con `@import 'components/_search.css'`

### Componentes Razor

- [x] `Pages/Shared/Components/_DsClientSearch.cshtml` — barra global + dropdown
- [x] `Pages/Shared/Components/_DsClientList.cshtml` — panel lateral (Aside)

### JS

- [x] `wwwroot/js/ds/ds-search.js` — módulo `DS.search` completo

### Pendiente

- [ ] Registrar `ds-search.js` en `_Layout.cshtml` (o en la página que lo consuma)
- [ ] Añadir endpoint API `GET /api/clientes/buscar?q=&estado=&top=` al controller/page
- [ ] Añadir endpoint API `GET /api/clientes?estado=` para la lista lateral
- [ ] Integrar `_DsClientSearch` y `_DsClientList` en la página consumidora
- [ ] Testing de accesibilidad: teclado (↑↓ Enter Escape) y lector de pantalla
- [ ] Testing responsive (breakpoints md / sm)

---

## 💾 Uso básico en una Razor Page

```cshtml
@* 1. En la sección de estilos específicos de la página (si se necesita override) *@
@section Styles {
    @* _search.css ya es importado globalmente por ds.css *@
}

@* 2. Barra de búsqueda global (zona superior) *@
@{
    ViewData["SearchFilter"]  = "por-verificar";
    ViewData["TotalTodos"]    = Model.TotalTodos;
    ViewData["TotalPorVerif"] = Model.TotalPorVerificar;
    ViewData["TotalVerif"]    = Model.TotalVerificados;
}
<partial name="Shared/Components/_DsClientSearch" />

@* 3. Layout de dos columnas *@
<div class="d-flex gap-3" style="height:calc(100vh - 200px)">

    @* Aside — lista de clientes *@
    <div style="width:426px;flex-shrink:0">
        @{
            ViewData["ClientListTitle"] = "Resultados";
            ViewData["ClientListCount"] = Model.TotalPorVerificar;
            ViewData["ClientListTotal"] = Model.TotalTodos;
        }
        <partial name="Shared/Components/_DsClientList" />
    </div>

    @* Panel de detalle *@
    <div class="flex-grow-1">
        @* tu panel de detalle aquí *@
    </div>

</div>

@* 4. Script al final *@
@section Scripts {
    <script src="~/js/ds/ds-search.js"></script>
    <script>
        DS.events.on('cliente:seleccionado', ({ id }) => {
            // cargar detalle del cliente seleccionado
            console.log('Cliente seleccionado:', id);
        });
    </script>
}
```

---

## 🗂 Archivos entregados

| Archivo | Tipo | Descripción |
| :--- | :--- | :--- |
| `wwwroot/css/ds/components/_search.css` | CSS | Estilos del componente search |
| `wwwroot/js/ds/ds-search.js` | JS | Lógica de búsqueda (`DS.search`) |
| `Pages/Shared/Components/_DsClientSearch.cshtml` | Partial | Barra global + dropdown |
| `Pages/Shared/Components/_DsClientList.cshtml` | Partial | Panel lateral de clientes |
| `wwwroot/css/variables.css` | Token | +2 tokens nuevos |
| `tailwind.config.js` | Token | Extensión de paleta y border |
| `wwwroot/css/ds/ds.css` | Entry | Import de `_search.css` |
