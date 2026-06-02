# Plan de Trabajo — Optimización CSS por Página

**Estado:** Pendiente  
**Prioridad:** Media  
**Prerequisito:** Design System estable (`variables.css` + `ds/components/`)

---

## Contexto

El Design System en `wwwroot/css/ds/` está bien organizado y usa tokens de `variables.css`. El problema son los archivos CSS específicos por página que se cargaron antes o en paralelo al DS y acumulan:

- Valores hardcodeados que deben venir de `variables.css`
- Reglas que duplican o sobreescriben componentes del DS innecesariamente
- Selectores excesivamente específicos que dificultan la herencia del DS
- Dead code de iteraciones anteriores

### Archivos a auditar (por tamaño, mayor a menor)

| Archivo | Tamaño | Riesgo estimado |
|---|---|---|
| `dashboard.css` | ~50 KB | Alto |
| `clientes.css` | ~39 KB | Alto |
| `bulkupload.css` | ~27 KB | Medio |
| `users.css` | ~17 KB | Medio |
| `login.css` | ~13 KB | Bajo (página aislada) |
| `layout.css` | ~8 KB | Bajo (global, con cuidado) |

---

## Criterios de Auditoría

Por cada archivo, buscar y reemplazar los siguientes patrones:

### 1. Colores hardcodeados — reemplazar con variables

```css
/* MAL */
color: #446942;
background: #295136;
border-color: rgba(68,105,66,0.2);

/* BIEN */
color: var(--color-primary);
background: var(--color-primary-dark);
border-color: var(--color-primary-10);
```

Referencia de tokens disponibles en `variables.css`:
- `--color-primary`, `--color-primary-dark`, `--color-primary-light`
- `--color-primary-5` … `--color-primary-60` (opacidades)
- `--color-success`, `--color-warning`, `--color-danger`, `--color-info`
- `--color-text-primary`, `--color-text-secondary`, `--color-text-muted`
- `--color-bg-*`, `--color-border-*`

### 2. Espaciados hardcodeados — reemplazar con variables

```css
/* MAL */
padding: 16px;
margin-bottom: 24px;
gap: 12px;

/* BIEN */
padding: var(--spacing-4);
margin-bottom: var(--spacing-6);
gap: var(--spacing-3);
```

Escala disponible: `--spacing-1` (4px) … `--spacing-12` (48px)  
Aliases semánticos: `--spacing-section`, `--spacing-card`, `--spacing-inline`, `--spacing-element`

### 3. Tipografía hardcodeada — reemplazar con variables

```css
/* MAL */
font-size: 14px;
font-weight: 600;
font-family: 'Inter', sans-serif;

/* BIEN */
font-size: var(--font-size-sm);
font-weight: var(--font-weight-semibold);
font-family: var(--font-body);
```

### 4. Sombras y bordes — reemplazar con variables

```css
/* MAL */
box-shadow: 0 2px 4px rgba(0,0,0,0.1);
border-radius: 8px;

/* BIEN */
box-shadow: var(--shadow-card);
border-radius: var(--radius-lg);
```

### 5. Reglas duplicadas del DS — eliminar o delegar

Si una regla en `[pagina].css` reproduce lo que ya hace `.ds-btn`, `.ds-card`, `.ds-table`, etc., la regla de página debe eliminarse. La página debe usar las clases del DS directamente en el HTML.

---

## Proceso de Ejecución por Archivo

Para cada archivo de la lista:

1. **Abrir el archivo y contar instancias de colores hex** (`#[0-9a-fA-F]{3,6}`).
2. **Buscar valores de spacing/font-size numéricos** (`\d+px` que no sean 0 ni 1px de borde).
3. **Identificar selectores que apuntan a clases del DS** (`.ds-*`) y evaluar si la regla es una extensión legítima o una sobreescritura innecesaria.
4. **Reemplazar** usando las tablas de la sección anterior.
5. **Verificar visualmente** la página afectada antes y después.
6. **Registrar reducción de líneas** en la tabla de seguimiento al pie.

---

## Reglas de Trabajo

- No tocar `variables.css` ni los archivos en `ds/components/` — son la fuente de verdad.
- No tocar `bootstrap-overrides.css` hasta tener un plan de migración Bootstrap → DS aprobado.
- `layout.css` y `site.css` son globales — cualquier cambio requiere prueba completa en todas las páginas.
- `login.css` es página aislada y de bajo riesgo — se puede abordar independientemente.
- Hacer un commit por archivo auditado para facilitar rollback.

---

## Tabla de Seguimiento

| Archivo | Líneas antes | Líneas después | Reducción | Responsable | Estado |
|---|---|---|---|---|---|
| `dashboard.css` | ~1400 | — | — | — | Pendiente |
| `clientes.css` | ~1100 | — | — | — | Pendiente |
| `bulkupload.css` | ~750 | — | — | — | Pendiente |
| `users.css` | ~480 | — | — | — | Pendiente |
| `login.css` | ~365 | — | — | — | Pendiente |
| `layout.css` | ~230 | — | — | — | Pendiente |

---

## Información Faltante

- [ ] Confirmar si `bootstrap-overrides.css` puede eliminarse (indica qué páginas aún dependen de clases Bootstrap legacy).
- [ ] Definir si el objetivo es llegar a 0 uso de Bootstrap en páginas nuevas.
- [ ] Indicar si hay páginas en desarrollo activo — priorizarlas al final para no generar conflictos.
