# Plan de Releases — Plataformas de Gestión de Datos

**Fecha de revisión:** 02 de junio de 2026  
**Estado:** En desarrollo activo

---

## Plataformas del Departamento de Gestión de Datos

### DSH 

Sistema web interno para la verificación de contactabilidad de clientes. Permite a los operadores localizar, validar y registrar información de contacto y ubicación de clientes que no han podido ser alcanzados por los canales convencionales. Integra autenticación corporativa, gestión de datos bajo normativa LOPDP y trazabilidad completa de operaciones.

### Slider

Motor interno de deduplicación y consolidación de identidades de clientes. Vive dentro del monolito DSH como un servicio de dominio especializado, sin interfaz propia.

Problema que resuelve: un mismo cliente puede ser registrado múltiples veces bajo distintos tipos de documento. El caso más frecuente es que un cliente exista con su cédula de identidad (10 dígitos) y que posteriormente sea creado de nuevo con su RUC de persona natural (los mismos 10 dígitos más el sufijo "001"). El sistema los trata como dos clientes distintos pero en realidad es la misma persona.

Mecanismo: cuando se crea o actualiza un registro en el maestro de clientes, Slider aplica un algoritmo de concordancia que normaliza el número de identificación (extrae la base de 10 dígitos independientemente del tipo de documento) y lo compara contra los registros existentes. Si detecta una coincidencia, vincula ambos registros mediante un identificador canónico y consolida la información de contactos y direcciones hacia el registro maestro.

Modelo de datos requerido: agregar la columna `Id_Cliente_Canonico` (BIGINT, FK NULL, autorreferencia sobre `operativo.Tbl_Maest_Cliente`) para identificar cuál es el registro maestro. Un valor NULL indica que el registro es en sí mismo el canónico. Un valor no nulo apunta al registro que concentra la información consolidada. Esto implica una migración EF Core y la actualización del diccionario de datos.

---

## Plataforma de Gestión de Datos (PGD) — Historial y Roadmap

---

### Release 1.0.0 — MVP (RF:2026-053) — ENTREGADO

**Objetivo:** Poner en operación la capacidad mínima para que los equipos de contactabilidad gestionen y verifiquen información de clientes desde la red interna.

**Funcionalidades entregadas:**

- Autenticación mediante grupos del directorio de identidades corporativo. Cuatro perfiles operativos: ADMIN, SUPERVISOR, AGENTE, CONSULTA.
- Dashboard principal con búsqueda de clientes por nombre o identificación. La búsqueda requiere mínimo tres caracteres y muestra sugerencias en tiempo real desde caché en memoria.
- Verificación de contactabilidad desde el dashboard: el operador puede aprobar o rechazar la información de contacto de un cliente, con selección de código de motivo en caso de rechazo.
- Ficha completa del cliente con gestión de contactos (celular, correo, convencional) y direcciones, incluyendo coordenadas GPS y visualización en mapa interactivo.
- Barra de progreso en el dashboard: total de clientes, pendientes de verificar y verificados.
- Módulo de administración de usuarios (exclusivo perfil ADMIN).
- Estructura de datos y elementos visuales para consentimiento LOPDP por cada valor de contacto.

**Brechas conocidas en este release:**

- El módulo de carga masiva (BulkUpload) no es operativo. La extracción de identificaciones desde Excel/CSV funciona, pero el procesamiento real contra la base de datos está simulado con un retardo ficticio y errores mock.
- El flujo de consentimiento LOPDP está incompleto: la columna, el FK a catálogo y los elementos visuales existen, pero la lógica de negocio que bloquea la verificación ante un rechazo no está implementada.
- La tabla `Tbl_Oficializacion_Core` existe con su entidad y configuración EF Core, pero no hay ningún servicio que ejecute el envío real de datos al Core bancario.

---

### Release 1.1.0 — Compliance, Operaciones y DT (pendiente de fecha)

**Objetivo:** Cerrar las brechas de compliance del MVP, activar el módulo de carga masiva real y sanear la deuda técnica acumulada antes de incorporar nuevas capacidades.

**Deuda Técnica incluida en este release:**

1. `ClientFinancial.cs` — La propiedad `ClientId` (string) almacena el valor de `Tipo_Contabilidad` por un error de nomenclatura. Renombrar a `AccountingType` y actualizar la configuración EF Core y todos los puntos de uso.
   Archivos: `CI_API/SDH.Domain/Entities/Operative/ClientFinancial.cs`, `CI_API/SDH.infrastructure/Persistence/DataConfigurations/Operativo/FinancieroClienteConfiguration.cs`

2. `CustomerAddresses.Source` — El campo de origen está hardcodeado a `GlobalVariables.SystemUser` en el factory method. Debe recibir el origen como parámetro, alineándose con el comportamiento de `CustomerContacts.Create()`.
   Archivo: `CI_API/SDH.Domain/Entities/Operative/CustomerAddresses.cs`

3. `ClienteConfiguration.cs` — `Identificacion_Cliente` tiene `HasMaxLength(32)` en la configuración EF Core pero la columna SQL está definida como NVARCHAR(20). Corregir a 20.
   Archivo: `CI_API/SDH.infrastructure/Persistence/DataConfigurations/Operativo/ClienteConfiguration.cs` línea 27

4. `CustomerAddressConfiguration.cs` — El índice `IX_Contacto_Cliente_IdCliente_EstaEliminado` referencia en su nombre la tabla de contactos pero está definido sobre la tabla de direcciones. Renombrar a `IX_Direccion_Cliente_IdCliente_EstaEliminado`.
   Archivo: `CI_API/SDH.infrastructure/Persistence/DataConfigurations/Operativo/CustomerAddressConfiguration.cs` línea 153

5. `Tbl_Direccion_Cliente.estado_LOPDP` — Columna en minúsculas, incumpliendo la convención Snake_Pascal_Case del esquema. Renombrar a `Estado_LOPDP` mediante migración EF Core.

6. Unificación del prefijo de artefactos — Los proyectos del repositorio usan el prefijo `SDH` (ej. `SDH.Domain`, `SDH.infrastructure`, `SDH.Application`) mientras que el nombre oficial de la plataforma es `DSH`. Renombrar todos los artefactos al prefijo `DSH` para eliminar la ambigüedad. Implica renombrar carpetas, namespaces, referencias en `*.csproj` y `*.sln`, y actualizar cualquier script o pipeline que referencie los nombres actuales.

**Módulo LOPDP completo:**

- Flujo de registro de consentimiento por cada dato de contacto: el operador confirma si el cliente acepta o rechaza la ley de protección de datos, con registro de fecha y hora por cada valor.
- Bloqueo de verificación: si el cliente rechaza el consentimiento LOPDP para un medio de contacto, el sistema impide la verificación de ese dato y muestra un mensaje explicativo.
- Visualización del estado LOPDP en la ficha del cliente por cada contacto y dirección.

**Módulo de carga masiva operativo:**

- Procesamiento real de archivos Excel/CSV: búsqueda por lote de identificaciones, presentación de coincidencias y registro de los que no existen en la base de datos.
- El campo `Source` se registra correctamente como identificador del proceso batch en el momento de la carga.

**Geolocalización asistida:**

- Desde la ficha del cliente, el operador captura las coordenadas mediante la API de geolocalización del navegador y las asigna a la dirección activa, eliminando la necesidad de introducirlas manualmente. Requiere HTTPS en el entorno de producción.

**Nota de alcance:** La integración con el Core bancario (Oficializacion_Core) se incluirá en este release únicamente si el equipo Core habilita el endpoint receptor antes del cierre del desarrollo. De lo contrario, se gestiona como tarea independiente.

---

### Release 1.2.0 — Visibilidad, Reportes y Novedades (pendiente de fecha)

**Objetivo:** Proveer a supervisores y administradores visibilidad sobre el rendimiento operativo y equipar a todos los usuarios con un mecanismo de novedades que comunique los cambios de la plataforma versión a versión.

**Dashboard de supervisión:**

- Panel exclusivo para perfiles ADMIN y SUPERVISOR con métricas consolidadas: clientes verificados, no verificados y pendientes, segmentados por operador y por rango de fechas.
- Indicadores de actividad del equipo: volumen de gestiones por operador en el período seleccionado.

**Reportes operativos:**

- Exportación en formato Excel de los resultados de verificación: cliente, operador, fecha y hora, resultado (verificado/rechazado) y motivo de rechazo cuando aplica.
- Filtros disponibles: por operador, por estado de verificación y por rango de fechas.

**Módulo de Changelog / Novedades:**

El módulo notifica a cada usuario los cambios de la plataforma al momento de la primera sesión posterior a una nueva versión, y mantiene un historial consultable de todos los releases.

Modelo de datos requerido:

- `parametro.Tbl_Release_Nota` (Id_Release INT PK, Version NVARCHAR(20), Titulo NVARCHAR(150), Descripcion NVARCHAR(500), Fecha_Publicacion DATE, Esta_Activo BIT): Registra cada versión publicada con su resumen general.
- `parametro.Tbl_Release_Nota_Item` (Id_Item INT PK, Id_Release INT FK, Categoria NVARCHAR(50), Descripcion NVARCHAR(300), Orden INT): Detalla las funcionalidades o correcciones incluidas en cada versión. Las categorías posibles son: Nueva funcionalidad, Mejora, Corrección, Cambio de diseño.
- `seguridad.Tbl_Usuario_Release_Vista` (Id_Usuario INT FK, Version NVARCHAR(20), Fecha_Vista DATETIME2): Registra qué versiones ha visto cada usuario y cuándo. Clave compuesta (Id_Usuario, Version).

Comportamiento esperado:

- Al iniciar sesión, la plataforma compara las versiones publicadas en `Tbl_Release_Nota` con los registros de `Tbl_Usuario_Release_Vista` del usuario. Si existen versiones no vistas, muestra un modal de "Novedades" con el detalle de los cambios.
- El usuario puede cerrar el modal o marcarlo como leído. En ambos casos se registra la versión en `Tbl_Usuario_Release_Vista`.
- Una página `/novedades` accesible desde el menú principal permite consultar el historial completo de versiones con sus items agrupados por categoría.
- La carga de los datos de cada release se realiza mediante seeders versionados, de modo que el contenido queda en control de versiones junto con el código.

---

### Release 1.3.0 — Slider: Motor de Deduplicación de Clientes (pendiente de fecha)

**Objetivo:** Incorporar el motor Slider dentro del monolito para detectar y consolidar registros duplicados del maestro de clientes originados por diferencias en el tipo de documento de identidad.

**Prerequisito de datos:** Migración que agrega la columna `Id_Cliente_Canonico` (BIGINT, FK NULL autorreferencia) a `operativo.Tbl_Maest_Cliente` y actualiza el diccionario de datos. Esta migración puede prepararse durante 1.2.0 para no bloquear el inicio del desarrollo.

**Algoritmo de concordancia:**

- Cuando se crea o actualiza un cliente, Slider normaliza el número de identificación: si el documento es un RUC de persona natural (13 dígitos terminados en "001"), extrae los primeros 10 dígitos para obtener el equivalente de cédula.
- Compara el valor normalizado contra los registros activos del maestro de clientes con tipo de documento diferente.
- Si existe una coincidencia, establece cuál registro es el canónico (generalmente el más antiguo o el que tiene más información completa) y actualiza `Id_Cliente_Canonico` en los registros secundarios.

**Consolidación de información:**

- Los contactos y direcciones de los registros secundarios se migran al registro canónico, conservando el campo `Source` original para mantener trazabilidad del origen.
- Los registros secundarios permanecen visibles en la base de datos con su `Id_Cliente_Canonico` poblado, pero la plataforma los excluye de los resultados de búsqueda y los presenta únicamente como referencias históricas desde la ficha del cliente canónico.

**Ejecución:**

- Procesamiento en tiempo real al crear o modificar un cliente (sin bloquear la operación del operador — asíncrono o en background).
- Proceso de conciliación en lote (batch) ejecutable bajo demanda por el perfil ADMIN para analizar el histórico existente antes de la activación del motor.

**Interfaz en la ficha del cliente:**

- La ficha del cliente canónico muestra un indicador de identidades vinculadas, con acceso al listado de los registros que han sido consolidados bajo ese cliente.

---

### Release 2.0.0 — Evolución Arquitectural (pendiente de fecha)

**Objetivo:** Separar la plataforma en capas independientes desplegables, habilitar el despliegue continuo y escalar la capacidad de ingesta de datos externos.

**Prerequisito:** Completar los releases 1.1.0, 1.2.0 y 1.3.0. Producir un documento de arquitectura que defina la estrategia de despliegue, los contratos de API y el modelo de datos del esquema carga antes de iniciar el desarrollo.

**Desacoplamiento frontend/backend:**

- El frontend migra hacia una interfaz que consume la API REST de forma explícita, desacoplándose del ciclo de vida del servidor Razor Pages.
- El backend se despliega como aplicación auto-hospedada (Kestrel), eliminando la dependencia de IIS y facilitando la integración con pipelines CI/CD.
- La API REST existente se formaliza con contrato OpenAPI/Swagger como punto de entrada oficial para integraciones externas.

**Esquema de carga de datos externos:**

- Diseño e implementación del esquema staging `carga` para la ingesta de archivos de proveedores externos autorizados (bureaus de crédito).
- Proceso ETL: validación de formato, transformación al esquema operativo y promoción controlada de registros. Este componente es el soporte técnico del procedimiento operativo 4.1 que actualmente no tiene implementación.

**Vistas operativas:**

- Implementar `operativo.vw_Contactabilidad_Clientes` y `operativo.vw_Clientes_Contactos_Detalle`, documentadas en el Diccionario de Datos pero sin scripts SQL ni configuración EF Core.

---

## Paquetes de Liberación

### Release 1.1.0 — Paquete A: Deuda Técnica y LOPDP

Alcance: Los 5 items de deuda técnica, flujo LOPDP completo con bloqueo de verificación, módulo de carga masiva operativo con asignación correcta del campo Source.  
Condición de salida: Migración EF generada y aplicada en ambiente de prueba sin regresiones; el operador puede registrar y consultar consentimiento LOPDP; un archivo Excel cargado produce resultados reales.  
Riesgo: Medio. La deuda técnica incluye renombrado de propiedades de dominio con impacto en capas de aplicación e infraestructura.

### Release 1.1.0 — Paquete B: Geolocalización Asistida

Alcance: Captura de coordenadas del dispositivo del operador desde la ficha del cliente mediante la API del navegador.  
Puede desarrollarse en paralelo con el Paquete A.  
Condición de salida: El operador puede registrar coordenadas sin introducción manual.  
Riesgo: Bajo. Funcionalidad de fronted sobre estructura de datos ya existente; requiere HTTPS en producción.

---

### Release 1.2.0 — Paquete único: Reportes, Dashboard y Changelog

Alcance: Dashboard de supervisión con métricas por operador y fecha; exportación Excel de verificaciones; tablas `Tbl_Release_Nota`, `Tbl_Release_Nota_Item` y `Tbl_Usuario_Release_Vista`; modal de novedades en login; página `/novedades`; seeder del changelog de los releases 1.0.0, 1.1.0 y 1.2.0.  
Depende de: Release 1.1.0 completado (los reportes requieren datos LOPDP correctos para ser válidos).  
Condición de salida: Un supervisor descarga un reporte filtrado; un usuario que ingresa por primera vez después de la publicación ve el modal de novedades y puede cerrarlo o consultarlo luego en la página de historial.  
Riesgo: Bajo. Funcionalidad de lectura sobre datos existentes más estructuras nuevas sin impacto en flujos operativos críticos.

---

### Release 1.3.0 — Paquete único: Motor Slider

Alcance: Migración que agrega `Id_Cliente_Canonico` a `Tbl_Maest_Cliente`; servicio Slider con algoritmo de concordancia cédula/RUC; proceso batch de conciliación histórica; integración en el flujo de creación/actualización de clientes; visualización de identidades vinculadas en la ficha del cliente; seeder del changelog para 1.3.0.  
Depende de: Release 1.2.0 completado.  
Condición de salida: Al crear un cliente con RUC cuya cédula base ya existe en el sistema, Slider vincula ambos registros y la ficha del cliente canónico muestra las identidades consolidadas.  
Riesgo: Medio-Alto. El algoritmo de concordancia debe validarse contra el universo de clientes existentes en el batch de conciliación antes de activar el procesamiento en tiempo real, para evitar vínculos incorrectos.

---

### Release 2.0.0 — Paquetes Pre, A, B y C

Paquete Pre: Documento de arquitectura (hosting, contratos API, modelo carga). Sin entregable de código. Condición de salida: documento aprobado por el equipo técnico.

Paquete A: Backend auto-hospedado Kestrel, contrato OpenAPI/Swagger, pipeline CI/CD básico. Riesgo: Alto. Cambio de infraestructura de despliegue con coordinación requerida.

Paquete B: Esquema carga, proceso ETL para proveedores externos, integración con procedimiento operativo 4.1. Riesgo: Medio. Depende de la especificación de formato entregada por el proveedor externo. Puede desarrollarse en paralelo con Paquete A.

Paquete C: Formalización de las vistas `vw_Contactabilidad_Clientes` y `vw_Clientes_Contactos_Detalle` con scripts SQL y configuración EF Core. Riesgo: Bajo. Riesgo: Bajo. Puede desarrollarse en paralelo con Paquetes A y B.
