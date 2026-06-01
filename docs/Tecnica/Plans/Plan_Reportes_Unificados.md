# Plan de Trabajo — Estilo Unificado de Reportes

**Estado:** Pendiente  
**Prerequisito:** Logo disponible en `docs/assets/logo.png` (ya creada la carpeta)

---

## Objetivo

Todos los reportes del proyecto (vulnerabilidades, auditorías, operacionales) deben tener:
- Header con logo del banco
- Tipografía y paleta de colores tomadas de `variables.css`
- Estructura de secciones consistente
- Pie de página con metadatos del documento

---

## Estructura de Carpetas

```
docs/
├── assets/
│   ├── logo.png          ← Logo oficial (ya existe)
│   ├── logo-white.png    ← Versión en blanco para fondos oscuros (opcional)
│   └── favicon.ico       ← Favicon para vistas HTML (opcional)
│
├── templates/
│   ├── report.html       ← Plantilla base HTML para reportes
│   └── report.css        ← Estilos de reporte (extiende tokens del DS)
│
├── vulnerabilidad/
│   └── 20260528_informe.md
└── Tecnica/
    └── Runbook.md
```

---

## Fase 1 — Plantilla HTML Base

Crear `docs/templates/report.html` con la siguiente estructura:

```html
<!DOCTYPE html>
<html lang="es">
<head>
  <meta charset="UTF-8">
  <title>{{TITULO_REPORTE}}</title>
  <link rel="stylesheet" href="../templates/report.css">
</head>
<body>
  <!-- HEADER -->
  <header class="rpt-header">
    <div class="rpt-header__meta">
      <h1 class="rpt-title">{{TITULO_REPORTE}}</h1>
      <p class="rpt-subtitle">{{SUBTITULO}}</p>
      <div class="rpt-header__info">
        <span class="rpt-label">Versión</span><span class="rpt-value">{{VERSION}}</span>
        <span class="rpt-label">Fecha</span><span class="rpt-value">{{FECHA}}</span>
        <span class="rpt-label">Responsable</span><span class="rpt-value">{{RESPONSABLE}}</span>
      </div>
    </div>
    <div class="rpt-header__logo">
      <img src="../assets/logo.png" alt="Data Smart Hub" height="52">
    </div>
  </header>

  <!-- CONTENIDO -->
  <main class="rpt-body">
    <section class="rpt-section">
      <h2>1. Título de sección</h2>
      <p>Contenido...</p>
    </section>
  </main>

  <!-- FOOTER -->
  <footer class="rpt-footer">
    <span>Data Smart Hub — Uso interno</span>
    <span>{{FECHA}}</span>
    <span class="rpt-footer__page">Página <span class="page-number"></span></span>
  </footer>
</body>
</html>
```

---

## Fase 2 — Hoja de Estilos de Reportes

Crear `docs/templates/report.css`. Usar los tokens del DS vía variables CSS inline (no depende de que el servidor sirva `variables.css`):

```css
:root {
  /* Tokens replicados para uso offline — sincronizar con variables.css */
  --rpt-primary:       #446942;
  --rpt-primary-dark:  #295136;
  --rpt-primary-light: #7F9787;
  --rpt-text:          #1a1a1a;
  --rpt-text-muted:    #6b7280;
  --rpt-border:        #e5e7eb;
  --rpt-bg-alt:        #f9fafb;
  --rpt-font-body:     'Inter', 'Segoe UI', sans-serif;
  --rpt-font-display:  'Sora', 'Segoe UI', sans-serif;
}

/* Reset mínimo */
*, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }
body { font-family: var(--rpt-font-body); font-size: 14px; color: var(--rpt-text); }

/* Header: título a la izquierda, logo en esquina superior derecha */
.rpt-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 24px;
  padding: 20px 32px;
  border-bottom: 3px solid var(--rpt-primary);
  background: white;
}
.rpt-header__meta { flex: 1; }
.rpt-header__logo { flex-shrink: 0; }
.rpt-header__logo img { display: block; }
.rpt-title { font-family: var(--rpt-font-display); font-size: 20px; color: var(--rpt-primary-dark); }
.rpt-subtitle { font-size: 13px; color: var(--rpt-text-muted); margin-top: 4px; }
.rpt-header__info { display: flex; flex-wrap: wrap; gap: 4px 16px; font-size: 12px; margin-top: 10px; }
.rpt-label { color: var(--rpt-text-muted); font-weight: 600; text-transform: uppercase; letter-spacing: 0.05em; }
.rpt-value { color: var(--rpt-text); }

/* Body */
.rpt-body { padding: 32px; max-width: 1000px; margin: 0 auto; }
.rpt-section { margin-bottom: 32px; }
.rpt-section h2 {
  font-family: var(--rpt-font-display);
  font-size: 16px;
  color: var(--rpt-primary-dark);
  border-bottom: 1px solid var(--rpt-border);
  padding-bottom: 8px;
  margin-bottom: 16px;
}
.rpt-section h3 { font-size: 14px; color: var(--rpt-primary); margin: 16px 0 8px; }

/* Tablas */
.rpt-table { width: 100%; border-collapse: collapse; font-size: 13px; }
.rpt-table th { background: var(--rpt-primary-dark); color: white; padding: 8px 12px; text-align: left; }
.rpt-table td { padding: 8px 12px; border-bottom: 1px solid var(--rpt-border); }
.rpt-table tr:nth-child(even) td { background: var(--rpt-bg-alt); }

/* Badges de severidad */
.rpt-badge { display: inline-block; padding: 2px 8px; border-radius: 9999px; font-size: 11px; font-weight: 600; }
.rpt-badge--critical { background: #fee2e2; color: #991b1b; }
.rpt-badge--high     { background: #ffedd5; color: #9a3412; }
.rpt-badge--medium   { background: #fef9c3; color: #854d0e; }
.rpt-badge--low      { background: #dcfce7; color: #166534; }
.rpt-badge--info     { background: #dbeafe; color: #1e40af; }

/* Footer */
.rpt-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px 32px;
  font-size: 11px;
  color: var(--rpt-text-muted);
  border-top: 1px solid var(--rpt-border);
  margin-top: 48px;
}

/* Print */
@media print {
  .rpt-header { position: running(header); }
  .rpt-footer { position: running(footer); }
  @page { margin: 20mm; size: A4; }
}
```

---

## Fase 3 — Colocar Logo

1. Obtener el logo oficial del banco en formato PNG con fondo transparente.
2. Guardarlo en `docs/assets/logo.png`.
3. Versión alternativa en blanco (si aplica): `docs/assets/logo-white.png`.
4. Dimensiones recomendadas: 200x60px a 150dpi.

---

## Fase 4 — Migrar Reportes Existentes

Una vez validada la plantilla con el equipo:

| Reporte | Archivo actual | Acción |
|---|---|---|
| Vulnerabilidades 2026-05-28 | `docs/vulnerabilidad/20260528_informe.md` | Convertir a HTML usando la plantilla |
| Runbook | `docs/Tecnica/Runbook.md` | Mantener Markdown — agregar referencia a plantilla para versiones impresas |
| Futuros informes | — | Usar `docs/templates/report.html` como punto de partida |

---

## Información Faltante

- [ ] Definir si los reportes deben poder abrirse en navegador (HTML) o solo como PDF impreso.
- [ ] Definir quién aprueba el template antes de migrar reportes existentes.
