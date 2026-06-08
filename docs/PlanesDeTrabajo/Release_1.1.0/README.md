# Release 1.1.0 — Planes de Trabajo: Deuda Técnica

**Fecha:** 05 de junio de 2026  
**Estado general:** Planificado — listo para iniciar

---

## Índice de planes

| Plan | Items | Estimación | Puede iniciar |
|------|-------|------------|---------------|
| [DT_Plan_01_Esquema_Dominio.md](DT_Plan_01_Esquema_Dominio.md) | DT-01 al DT-05 | 2-3 días | Inmediatamente |
| [DT_Plan_02_Estandarizacion_Prefijos.md](DT_Plan_02_Estandarizacion_Prefijos.md) | DT-13 | 1-2 días | Inmediatamente |
| [DT_Plan_03_Seguridad_Testing.md](DT_Plan_03_Seguridad_Testing.md) | DT-08, DT-09, DT-10 | 5-8 días | DT-08 iniciar ya (lead time AD) |
| [DT_Plan_04_Cifrado_Herramientas.md](DT_Plan_04_Cifrado_Herramientas.md) | DT-11, DT-12 | 3-5 días | Inmediatamente |

**Total estimado:** 11-18 días (con paralelismo entre planes)

---

## Decisiones tomadas

Todas las decisiones bloqueantes están resueltas. El sprint puede iniciarse.

| # | Decisión | Valor |
|---|----------|-------|
| 1 | Nombre definitivo de la plataforma | **Smart Data Hub (SDH)** — el código conserva sus prefijos actuales. DT-06 cancelado. |
| 2 | Nombre de la librería de cifrado | `Bm.Security.Lib.SecretTool` (package NuGet: `bm.security.lib.secrettool`) |
| 3 | Nombre de la herramienta de secrets | `Bm.Security.App.SecretTool` (package NuGet: `bm.security.app.secrettool`) |
| 4 | Mecanismo de distribución | Azure Artifacts — feed NuGet privado del banco en Azure DevOps |

---

## Dependencias entre planes

```
Plan 01 (DT-01 a DT-05)  ← sin dependencias externas, iniciar primero
  └── DT-02 es prerequisito del Módulo Carga Masiva (Paquete A)
  └── DT-05 es prerequisito del Módulo LOPDP completo (Paquete A)

Plan 02 (DT-13)  ← sin dependencias de código, puede ejecutarse en paralelo con Plan 01
  └── Actualización de documentación y textos de UI únicamente

Plan 03 (DT-08, DT-09, DT-10)
  └── DT-08 tiene dependencia del equipo AD: iniciar coordinación ahora
  └── DT-09 (unit tests): hacer después de Plan 01 para cubrir los cambios de DT-01 y DT-02
  └── DT-10 (Selenium): puede iniciarse en paralelo con DT-09

Plan 04 (DT-11, DT-12)
  └── DT-11 (extracción librería Bm.Security.Lib): iniciar primero, sin dependencias
  └── DT-12 (renombrar herramienta): requiere que DT-11 Fase 1 esté completo
  └── Fase 5 de DT-11 (publicar en Azure Artifacts): coordinar con DevOps para configurar el feed
```

---

## Ítems que requieren coordinación externa

| Item | Equipo externo | Acción requerida | Urgencia |
|------|---------------|-----------------|----------|
| DT-08 | Directorio de Identidad (AD) | Renombrar grupos `G_DSH_*` → `GS_DSH_*` en certificación y desarrollo | Alta — iniciar coordinación esta semana |
| DT-11 / DT-12 | DevOps / Azure Artifacts | Crear o confirmar el feed NuGet privado `bm-packages` en Azure DevOps | Media — necesario para la Fase 5 de DT-11 |
| DT-13 | Comunicaciones internas | Notificar que el nombre oficial de la plataforma es SDH | Baja — post-implementación |
