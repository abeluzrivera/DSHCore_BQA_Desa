# Estrategia de Migración — DB_ODS (Smart Data Hub)

**Sistema:** Data Smart Hub  
**Clasificación:** Uso Interno — Restringido  
**Responsable:** Pedro Rivera  
**Versión:** 2.0  
**Fecha:** 2026-06-16  

---

Este documento es el índice de la estrategia de gestión de base de datos para `DB_ODS`. Está dividido en dos documentos operativos según el escenario de uso:

---

## Documento 1 — Creación y carga inicial

**Archivo:** [`db_ods_01_creacion_carga_inicial.md`](db_ods_01_creacion_carga_inicial.md)

Aplica cuando la base de datos `DB_ODS` **no existe** en el ambiente objetivo y debe crearse desde cero.

Cubre:
- Prerequisitos, aprobaciones CAB y backup previo.
- Capa 1: creación de estructura con `DB_ODS_1.0.0.sql` (schemas, tablas, vistas, índices).
- Capa 2: aplicación de todas las migraciones EF Core.
- Capa 3: carga masiva inicial desde `DB_CONTACTABILIDAD` (datos Equifax).
- Validación final, plan de rollback por capa y registro de ejecución.

---

## Documento 2 — Actualización y modificación

**Archivo:** [`db_ods_02_actualizacion.md`](db_ods_02_actualizacion.md)

Aplica cuando la base de datos `DB_ODS` **ya existe y está operativa** y se necesita modificar o enriquecer.

Cubre tres tipos de actualización:

| Tipo | Cuándo usar |
|---|---|
| **A. Migración EF Core** | Nuevo release con cambios de esquema desde el código .NET |
| **B. Recarga / enriquecimiento de datos** | Ejecución periódica del Job Equifax |
| **C. Hotfix DDL manual** | Cambios urgentes de esquema que no pueden esperar al ciclo de EF |

Incluye prerequisitos por tipo, revisión de scripts, monitoreo, rollback y consideraciones de seguridad y LOPDP.

---

## Decisión rápida

```
¿DB_ODS existe en el ambiente?
  ├── NO  →  db_ods_01_creacion_carga_inicial.md
  └── SÍ  →  db_ods_02_actualizacion.md
                ├── Hay nuevas migraciones EF  →  Tipo A
                ├── Hay nuevos datos Equifax    →  Tipo B
                └── Hotfix urgente de esquema  →  Tipo C
```
