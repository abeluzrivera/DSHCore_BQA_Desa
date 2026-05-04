# 🧠 PROMPT MAESTRO — Plan de Implementación de Componente (Figma → Código)

> **Versión:** 2.0.0 | **Optimizado para:** GitHub Copilot, Cursor & AI Agents

## Objetivo

Tu tarea es analizar un diseño de Figma y el proyecto destino, y generar un **Plan de Implementación Técnico** ultra-detallado en Markdown.

Este plan será la **fuente de la verdad** para que otro agente de IA (o un desarrollador junior) implemente el componente **sin tomar decisiones de diseño, arquitectura ni estilo**. Toda decisión debe quedar resuelta en este documento.

**El plan se debe guardar en:** `docs/plan_componente_[COMPONENT_SLUG].md`

---

## 📋 CONFIGURACIÓN DE SESIÓN

Completa estas variables antes de iniciar:

| Variable | Valor / Ruta |
| :--- | :--- |
| **PROJECT_PATH** | `[Ruta raíz del proyecto]` |
| **FIGMA_URL** | `[https://www.figma.com/design/lJSE2H8XeNXlNRFAcJWYzd/Data-Smart-Hub?node-id=242-2375&m=dev]` |
| **COMPONENT_NAME** | `Detall de contactabilidad` |
| **COMPONENT_SLUG** | `Cliente` |
| **OUTPUT_FRAMEWORK** | `HTML o Razor (.cshtml)` |
| **STYLING_SYSTEM** | `Tailwind CSS` |
| **TOKEN_STRATEGY** | `extend` |
| **DESIGN_SYSTEM_REFS** | `[Rutas: tailwind.config.js, tokens CSS, carpeta de componentes reutilizables]` |
| **SESSION_MODE** | `multi` |

---

## 🚫 REGLAS NO NEGOCIABLES

### 1) Sistema de Diseño (Design System)

- Reutilizar el sistema de diseño existente en el proyecto. Escanea `PROJECT_PATH` antes de proponer cualquier valor.
- **Prohibido:** inventar colores, tipografías, sombras, escalas de espaciado o componentes custom.
- **Prohibido:** valores arbitrarios de Tailwind (`bg-[#xxx]`, `w-[123px]`, `p-[7px]`). Todo debe mapearse a tokens del sistema.
- **Prohibido:** CSS global nuevo ni estilos inline (`style="..."`).
- Si falta un componente, componer/ensamblar usando los existentes y tokens actuales. Documentar la composición.

### 2) Reutilización de Código (DRY)

- Si ya existe un componente, helper, servicio o flujo similar en el proyecto → reutilizarlo explícitamente (indicar ruta del archivo y nombre).
- No duplicar lógica. Si se necesita extender algo existente, documentar qué se extiende y por qué.

### 3) Seguridad + Accesibilidad

- Incluir atributos ARIA, semántica HTML5, navegación por teclado y compatibilidad con lectores de pantalla.
- Validar inputs en cliente y servidor.
- Mitigar: XSS/HTML injection, validación de datos, control de errores.

---

## 🔎 FASE 1: Extracción Técnica (MCP Figma)

Extraer y documentar la **FICHA TÉCNICA DEL COMPONENTE**:

1. **Estructura Atómica:** Árbol completo de Auto Layout (padre → hijos), Padding, Gap, Resizing (Fill/Hug) y Constraints.
2. **Estilos Visuales:** Colores (HEX), Border-radius, Shadows, Tipografía (Family, Size, Weight, Line-Height) y Opacidad.
3. **Variantes y Estados:** Todos los estados del componente (Default, Hover, Active, Focus, Disabled, Loading, Error, Empty, Success) y subcomponentes.
4. **Iconos y Assets:** Lista de iconos/imágenes usados, nombres exactos y formato.
5. **Responsividad:** Comportamiento en breakpoints si existe en Figma (mobile, tablet, desktop).

---

## 🧱 FASE 2: Auditoría del Proyecto y Design Tokens

Antes de planificar, escanear `PROJECT_PATH`:

### 2.1 Inventario del proyecto

- Listar componentes UI existentes relevantes (ruta y nombre de archivo).
- Listar tokens existentes (`tailwind.config.js`, variables CSS, theme).
- Listar helpers/utilidades reutilizables relacionadas.

### 2.2 Mapeo Figma → Tokens

Para **cada valor visual** extraído en Fase 1, documentar en una tabla:

| Valor Figma (raw) | Token existente | Acción |
| :--- | :--- | :--- |
| `#1A73E8` | `text-primary-600` | Reutilizar |
| `16px` padding | `p-4` | Reutilizar |
| `#F5F5F5` | _(no existe)_ | Crear `bg-surface-light` en config |

- **Si TOKEN_STRATEGY = extend:** agregar tokens faltantes al config existente.
- **Si TOKEN_STRATEGY = create:** crear estructura en `design-system/tokens/`.
- **Si TOKEN_STRATEGY = auto-detect:** decidir según el estado del proyecto y documentar la decisión.

### 2.3 Lista de prohibiciones específicas

Declarar explícitamente qué NO se debe hacer al implementar este componente (basado en lo encontrado en el proyecto).

---

## 🔒 FASE 3: Safe Mode (Congelamiento de Datos)

- Una vez generada la ficha técnica (Fase 1 + 2), **no re-consultar Figma**.
- Si falta un dato menor, inferir basándose en el sistema de diseño y **documentar la inferencia**.
- Si falta un dato crítico que cambia la arquitectura, marcarlo como **⚠️ BLOQUEANTE** y detener el plan hasta resolverlo.

---

## 🧩 FASE 4: Arquitectura del Componente

### 4.1 Estructura de archivos

Definir exactamente qué archivos se crean o modifican:

```
📦 Archivos a crear/modificar:
├── [ruta/NuevoComponente.ext]     → CREAR — Componente principal
├── [ruta/SubComponente.ext]       → CREAR — Subcomponente X
├── [ruta/tailwind.config.js]      → MODIFICAR — Agregar tokens [lista]
├── [ruta/tokens.css]              → MODIFICAR — Agregar variables [lista]
└── [ruta/index o barrel]          → MODIFICAR — Exportar nuevo componente
```

### 4.2 Props / Parámetros / ViewModel

Definir con tipado estricto:

```
Nombre: [ComponentName]Props / [ComponentName]ViewModel
─────────────────────────────────────────
| Prop/Campo     | Tipo              | Requerido | Default   | Descripción          |
|----------------|-------------------|-----------|-----------|----------------------|
| title          | string            | sí        | —         | Título principal     |
| status         | 'ok'|'error'      | no        | 'ok'      | Estado visual        |
```

### 4.3 Estados visuales

Para cada estado, describir:
- Qué cambia visualmente (clases Tailwind exactas).
- Qué lo activa (evento o condición de datos).
- Qué muestra al usuario (texto, icono, comportamiento).

### 4.4 Eventos y comportamiento

- Lista de eventos que emite o escucha el componente.
- Flujo de datos: de dónde vienen los datos, cómo se transforman, a dónde van.

---

## ⚠️ FASE 5: Casos Borde y Prevención de Errores

Lista de escenarios que el implementador **debe manejar obligatoriamente**:

| # | Escenario | Mitigación | Ubicación (cliente/servidor) |
|---|-----------|------------|------------------------------|
| 1 | Datos vacíos / null | Mostrar estado empty con mensaje X | Cliente |
| 2 | Error de red / timeout | Mostrar estado error, retry button | Cliente |
| 3 | Contenido XSS en datos | Sanitizar antes de renderizar | Servidor + Cliente |
| 4 | Texto muy largo / overflow | Truncar con `truncate` + tooltip | Cliente |
| ... | ... | ... | ... |

---

## 🧠 FASE 6: Lógica dura (especificaciones Copy-Paste)

Para cada pieza de lógica no trivial, documentar:

- **Nombre de la función/método.**
- **Firma:** inputs, outputs, tipos.
- **Algoritmo:** pseudocódigo o código listo para copiar.
- **Ejemplos:** al menos 2 casos que pasan y 2 que fallan.
- **Manejo de errores:** qué excepción/error lanzar y cuándo.

---

## 📋 FASE 7: Plan de Implementación Atómico

Dividir en pasos **ultra-específicos** que otro agente de IA pueda ejecutar uno por uno, sin ambigüedades.

Cada paso debe tener este formato:

```
### Paso [N]: [Título descriptivo]

**Archivo(s):** `ruta/archivo.ext` → [CREAR | MODIFICAR]
**Depende de:** Paso [X] (o "Ninguno")
**Descripción:**
  Instrucción concreta de qué hacer. Sin ambigüedades.
  Si es código, incluir el snippet exacto o la estructura esperada.

**Criterio de verificación:**
  Cómo saber que este paso está completo y correcto.
  [Ej: "El componente renderiza sin errores y muestra el estado default"]
```

### Orden sugerido de fases:

1. **Tokens y config** — Agregar tokens/variables faltantes al sistema de diseño.
2. **Estructura base** — Crear archivos, definir Props/ViewModel con datos mock.
3. **Layout y estilos** — Implementar HTML/JSX/Razor con clases Tailwind.
4. **Estados visuales** — Implementar cada estado (loading, error, empty, success).
5. **Lógica y eventos** — Conectar comportamiento, validaciones, llamadas.
6. **Accesibilidad** — ARIA, teclado, semántica.
7. **Integración** — Conectar con datos reales, rutas, servicios.
8. **Verificación final** — Checklist de auditoría (tokens, a11y, DRY, seguridad).

---

## 📤 SALIDA — Instrucción de guardado

Guardar el plan completo como:

```
docs/plan_componente_[COMPONENT_SLUG].md
```

El documento debe incluir todas las fases anteriores resueltas, sin placeholders ni "por definir". Si algo no se puede resolver, marcarlo como **⚠️ BLOQUEANTE** con contexto suficiente para desbloquearlo.

---

## ✅ CHECKLIST DE AUDITORÍA PRE-ENTREGA

Antes de entregar el plan, verificar:

- [ ] Todos los valores Figma están mapeados a tokens (sin HEX/RGBA hardcodeados).
- [ ] Se reutilizan componentes existentes del proyecto (listados por ruta).
- [ ] Props/ViewModel definidos con tipos, defaults y descripciones.
- [ ] Todos los estados visuales documentados con clases exactas.
- [ ] Casos borde identificados con mitigaciones concretas.
- [ ] Lógica no trivial con firmas, algoritmos y ejemplos.
- [ ] Plan atómico sin pasos ambiguos.
- [ ] Cada paso tiene archivo destino, dependencias y criterio de verificación.
- [ ] Accesibilidad: ARIA, semántica HTML5, navegación por teclado.
- [ ] Seguridad: XSS, validación, sanitización.
- [ ] Archivo de salida indicado: `docs/plan_componente_[COMPONENT_SLUG].md`

---

> **Referencia de implementación real:** ver [`_ejemplos/data-smart-hub.md`](./_ejemplos/data-smart-hub.md) — instancia completa con variables rellenas para el proyecto Data Smart Hub.
