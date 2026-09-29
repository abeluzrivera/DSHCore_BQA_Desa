# Storytelling BI — Smart Data Hub

> Borrador de trabajo. Punto de partida para definir los dashboards antes de decidir qué datos extraer.

## 1. El porqué de DataHub

### El problema antes de DataHub

El Banco gestiona la contactabilidad de clientes a través de procesos dispersos:
datos de clientes, direcciones y contactos que llegan de distintas fuentes
(Equifax, cargas manuales, sistemas internos), sin un punto único de verdad.
Cada equipo termina construyendo su propia versión de "quién es el cliente"
y "cómo lo contactamos", con el riesgo de duplicidad, desactualización y
falta de trazabilidad sobre quién tocó qué dato y cuándo.

### Qué resuelve DataHub

Smart Data Hub (Contactabilidad Inteligente) nace como el punto central donde:

- Los datos de clientes, contactos y direcciones se **oficializan** (existe
  un estado de verdad, no solo un registro más).
- Las cargas externas (Equifax) entran a un área de **staging** (`carga`)
  antes de integrarse, evitando que datos sucios contaminen la operación.
- Cada acción queda sujeta a un modelo de **seguridad y roles**
  (ADMIN, SUPERVISOR, AGENTE, CONSULTA), lo que permite saber quién
  gestionó qué contacto.
- La información queda estructurada para ser **auditable** y **reportable**,
  no solo operable.

### Por qué esto importa para el negocio

DataHub no es solo un sistema transaccional: es la base que permite
responder preguntas de negocio que hoy son difíciles de contestar de forma
confiable:

- ¿Qué tan actualizada está la información de contacto de nuestra cartera?
- ¿Qué agentes/áreas están manteniendo mejor la calidad del dato?
- ¿Cuánto tiempo toma oficializar un cliente desde que se carga?
- ¿Dónde se están cayendo los intentos de contacto y por qué?

### El hilo narrativo para BI

La historia que los dashboards deben contar no es "cuántos registros hay",
sino:

1. **Confianza del dato** — de dónde viene, qué tan completo y vigente está.
2. **Actividad y gobierno** — quién interactúa con la información y con qué
   frecuencia (auditoría de contactos/direcciones ya existe como precedente
   con el reporte diario de actividad).
3. **Efectividad de la contactabilidad** — si el dato mejor gestionado se
   traduce en mejores resultados de contacto para el negocio.

Estos tres ejes (**Confianza → Gobierno → Efectividad**) son el eje narrativo
propuesto para ordenar los dashboards antes de definir métricas puntuales.

---

## Próximos pasos (pendiente de definir)

- [ ] Validar el eje narrativo con negocio.
- [ ] Identificar audiencias por dashboard (operación, supervisión, gerencia).
- [ ] Levantar el inventario de fuentes disponibles por eje.
- [ ] Definir métricas candidatas por eje antes de tocar diseño visual.
