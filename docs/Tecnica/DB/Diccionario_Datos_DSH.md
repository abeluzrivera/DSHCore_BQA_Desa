# Diccionario de Datos - Esquema de Parámetros y Catálogos del Sistema (DSH)

## 1. REGLAS GENERALES Y CONTEXTO DEL ESQUEMA DE PARÁMETROS

* **Motor de Base de Datos:** SQL Server (Compatibility Level 150)
* **Zona Horaria:** Todos los campos `datetime2` se almacenan con la hora local del servidor (sin zona horaria explícita).
* **Borrado Lógico:** No aplica en el esquema de parámetros. Se utiliza un indicador de estado activo/inactivo (`Esta_Activo`).
* **Nomenclatura:** Esquemas y tablas en PascalCase con prefijo `Tbl_` o `Vw_`. Columnas en PascalCase separadas por guion bajo (`Snake_Pascal_Case`).

## 2. MAPA DE RELACIONES CRÍTICAS (Atajos de Joins)
* `parametro.Tbl_Cat_Grupo.Id_Grupo_Catalogo` <-> `parametro.Tbl_Cat_Item.Id_Grupo_Catalogo` (Relación Maestro-Detalle. Un grupo agrupa N ítems o constantes).
* `parametro.Tbl_Cat_Item.Id_Item_Catalogo` <-> `operativo.Tbl_Maest_Cliente.Id_Tipo_Identificacion` (Mapea el tipo de documento del cliente).
* `parametro.Tbl_Cat_Item.Id_Item_Catalogo` <-> `operativo.Tbl_Contacto_Cliente.Id_Tipo_Contacto` (Define si es Celular, Correo, etc.).
* `parametro.Tbl_Cat_Item.Id_Item_Catalogo` <-> `operativo.Tbl_Contacto_Cliente.Id_Estado_Verificacion` (Estado del medio de contacto).

---

## 3. DEFINICIÓN DE TABLAS Y VISTAS

### TABLA: parametro.Tbl_Cat_Grupo (Agrupadores de catálogos y constantes de sistema)
* **Id_Grupo_Catalogo** (INT, PK, IDENTITY(1,1), NOT NULL): Identificador único auto-incrementable del grupo.
* **Nombre_Grupo** (NVARCHAR(100), UNIQUE, NOT NULL): Nombre único en texto usado por C# Enums para mapear constantes de la app.
* **Descripcion** (NVARCHAR(250), NULL): Explicación funcional del propósito del grupo.
* **Es_Sistema** (BIT, DEFAULT 0, NOT NULL): `1` = Crítico para el software (No modificable), `0` = Modificable por usuario.
* **Fecha_Creacion** (DATETIME2(3), DEFAULT getdate(), NOT NULL): Fecha de registro del grupo.
* **Usuario_Creacion** (NVARCHAR(50), NULL): Usuario que registró el grupo.
* **Fecha_Modificacion** (DATETIME2(3), NULL): Fecha de última actualización.
* **Usuario_Modificacion** (NVARCHAR(50), NULL): Usuario que realizó la última actualización.
* *Índices Críticos:* * `PK_Tbl_Cat_Grupo` (CLUSTERED) -> `Id_Grupo_Catalogo` ASC.
  * `UQ_GrupoCatalogo_NombreGrupo` (UNIQUE NONCLUSTERED) -> `Nombre_Grupo` ASC.

### TABLA: parametro.Tbl_Cat_Item (Valores y opciones específicas por cada grupo)
* **Id_Item_Catalogo** (INT, PK, NOT NULL): Identificador único global y fijo asignado al ítem. Clave foránea destino de tablas operativas.
* **Id_Grupo_Catalogo** (INT, FK, NOT NULL): Relacionado con `parametro.Tbl_Cat_Grupo(Id_Grupo_Catalogo)`.
* **Codigo_Valor** (NVARCHAR(20), NOT NULL): Código alfanumérico corto usado en lógica de backend (Ej: 'CED', 'ADMIN').
* **Texto_Visual** (NVARCHAR(100), NOT NULL): Etiqueta en lenguaje natural expuesta en los dropdowns/combos de la UI.
* **Orden_Visual** (INT, NOT NULL): Peso numérico que define la posición de ordenamiento en la interfaz gráfica.
* **Esta_Activo** (BIT, DEFAULT 1, NOT NULL): `1` = Disponible para uso, `0` = Deshabilitado (Baja lógica).
* **Fecha_Creacion** (DATETIME2(3), DEFAULT getdate(), NOT NULL): Fecha de registro del ítem.
* **Usuario_Creacion** (NVARCHAR(50), NULL): Usuario que registró el ítem.
* **Fecha_Modificacion** (DATETIME2(3), NULL): Fecha de última modificación de configuración.
* **Usuario_Modificacion** (NVARCHAR(50), NULL): Usuario que modificó el ítem.
* *Restricciones e Índices Críticos:*
  * `PK_Tbl_Cat_Item` (CLUSTERED) -> `Id_Item_Catalogo` ASC.
  * `FK_Grupo_Item` -> Foreign Key contra `parametro.Tbl_Cat_Grupo(Id_Grupo_Catalogo)`.
  * `UQ_ItemCatalogo_IdGrupoCatalogo_CodigoValor` (UNIQUE NONCLUSTERED) -> Evita códigos duplicados dentro de un mismo grupo.
  * `IX_ItemCatalogo_Buscador` (NONCLUSTERED) -> Claves: `Id_Grupo_Catalogo` ASC, `Esta_Activo` ASC. Incluye: `Codigo_Valor`, `Texto_Visual`, `Orden_Visual`. Optimiza la carga de combos desde el backend.

### VISTA: parametro.Vw_Cat_Detalle_General (Query Object optimizado con SCHEMABINDING)
* **Id_Item_Catalogo** (INT): Identificador del ítem (Origen: Tbl_Cat_Item).
* **Id_Grupo_Catalogo** (INT): Identificador del grupo (Origen: Tbl_Cat_Item).
* **Nombre_Grupo** (NVARCHAR(100)): Nombre del grupo vinculante (Origen: Tbl_Cat_Grupo).
* **Codigo_Valor** (NVARCHAR(20)): Código funcional (Origen: Tbl_Cat_Item).
* **Texto_Visual** (NVARCHAR(100)): Texto de UI (Origen: Tbl_Cat_Item).
* **Orden_Visual** (INT): Secuencia en combos (Origen: Tbl_Cat_Item).
* **Esta_Activo** (BIT): Estado de disponibilidad (Origen: Tbl_Cat_Item).
* **Es_Sistema** (BIT): Flag de protección (Origen: Tbl_Cat_Grupo).
* *Lógica de Join Interna:* `parametro.Tbl_Cat_Item i INNER JOIN parametro.Tbl_Cat_Grupo g ON i.Id_Grupo_Catalogo = g.Id_Grupo_Catalogo`.

## 2. REGLAS GENERALES Y CONTEXTO DEL ESQUEMA OPERATIVO
* **Ámbito del Módulo:** Centralización del perfil 360 del cliente (Identificación, Contactos, Ubicación y Capacidad Financiera) junto con la auditoría de integración con el Core Bancario/Financiero.
* **Llaves Primarias Principales:** Uso de `bigint IDENTITY(1,1)` para el maestro de clientes debido al volumen transaccional escalar; las tablas de detalle usan `int IDENTITY(1,1)` o `bigint IDENTITY(1,1)`.
* **Borrado Lógico:** Controlado estrictamente por la columna `Esta_Eliminado` (BIT). `1` = Registro eliminado lógicamente (excluir de consultas de negocio), `0` = Registro activo.
* **Privacidad y Cumplimiento:** Incluye banderas para control de anonimización y protección de datos bajo normativas LOPDP.

## 2. MAPA DE RELACIONES CRÍTICAS
* `operativo.Tbl_Maest_Cliente.Id_Cliente` <-> `operativo.Tbl_Contacto_Cliente.Id_Cliente` (Un cliente tiene muchos medios de contacto: celular, correo, convencional).
* `operativo.Tbl_Maest_Cliente.Id_Cliente` <-> `operativo.Tbl_Direccion_Cliente.Id_Cliente` (Un cliente tiene muchas direcciones físicas registradas).
* `operativo.Tbl_Maest_Cliente.Id_Cliente` <-> `operativo.Tbl_Financiero_cliente.Id_Cliente` (Un cliente tiene un perfil de segmentación contable).
* `operativo.Tbl_Maest_Cliente.Id_Cliente` <-> `operativo.Tbl_Oficializacion_Core.Id_Cliente` (Historial de sincronización del cliente hacia sistemas Core externos).
* `parametro.Tbl_Cat_Item.Id_Item_Catalogo` <-> `operativo.Tbl_Maest_Cliente.Id_Tipo_Identificacion` (Resolución del catálogo del tipo de documento).
* `parametro.Tbl_Cat_Item.Id_Item_Catalogo` <-> `operativo.Tbl_Contacto_Cliente.Id_Tipo_Contacto` (101 = Celular, 102 = Correo, 103 = Convencional).
* `parametro.Tbl_Cat_Item.Id_Item_Catalogo` <-> `operativo.Tbl_Contacto_Cliente.Id_Estado_Verificacion` (Resolución del estado de validez del contacto).

---

## 3. DEFINICIÓN DE TABLAS Y VISTAS

### TABLA: operativo.Tbl_Maest_Cliente (Entidad Maestra / Aggregate Root de Clientes)
* **Id_Cliente** (BIGINT, PK, IDENTITY(1,1), NOT NULL): Identificador único global e interno del cliente.
* **Identificacion_Cliente** (NVARCHAR(20), NOT NULL): Número de documento de identidad (Cédula, RUC, Pasaporte).
* **Id_Tipo_Identificacion** (INT, FK, NOT NULL): Relacionado con `parametro.Tbl_Cat_Item(Id_Item_Catalogo)`.
* **Nombre_Completo** (NVARCHAR(250), NULL): Nombres y apellidos completos del cliente.
* **Esta_Verificado** (BIT, NOT NULL): Estado de validación biométrica o de buró del cliente.
* **Esta_Aprobado** (BIT, NOT NULL): Estado transaccional/comercial para operar en la plataforma.
* **Esta_Eliminado** (BIT, NOT NULL): Flag de Soft-Delete. `1` = Eliminado, `0` = Activo.
* **Esta_Anonimizado** (BIT, NOT NULL): Flag de protección de datos. `1` = Datos personales ofuscados por derecho al olvido.
* **Fecha_Creacion** (DATETIME2(3), NOT NULL): Fecha de registro inicial en el sistema.
* **Usuario_Creacion** (NVARCHAR(100), NOT NULL): Operador, asesor o sistema que registró al cliente.
* **Fecha_Modificacion** (DATETIME2(3), NULL): Fecha de última actualización del perfil maestro.
* **Usuario_Modificacion** (NVARCHAR(100), NULL): Último usuario que editó el registro.
* **Fecha_Expiracion_Legal** (DATE, NULL): Fecha de caducidad del documento o vigencia legal del cliente.
* *Índices y Restricciones Críticas:*
  * `PK_Tbl_Maest_Cliente` (CLUSTERED) -> `Id_Cliente` ASC.
  * `FK_Cliente_TipoIdentificacion` -> Foreign Key contra `parametro.Tbl_Cat_Item(Id_Item_Catalogo)`.
  * `IX_Cliente_Identificacion` (UNIQUE NONCLUSTERED) -> Clave: `Identificacion_Cliente`. Filtro condicional: `WHERE Esta_Eliminado = 0 AND Esta_Anonimizado = 0`. Garantiza unicidad operativa.
  * `IX_Cliente_Nombre` (NONCLUSTERED) -> Clave: `Nombre_Completo`. Filtro condicional: `WHERE Esta_Eliminado = 0 AND Esta_Anonimizado = 0`. Optimiza la búsqueda predictiva por texto.

### TABLA: operativo.Tbl_Contacto_Cliente (Medios de Contactabilidad)
* **Id_Contacto_Cliente** (INT, PK, IDENTITY(1,1), NOT NULL): Identificador único del medio de contacto.
* **Id_Cliente** (BIGINT, FK, NOT NULL): Relacionado con `operativo.Tbl_Maest_Cliente(Id_Cliente)`.
* **Id_Tipo_Contacto** (INT, FK, NOT NULL): Relacionado con `parametro.Tbl_Cat_Item(Id_Item_Catalogo)` (Ej: Celular, Email).
* **Valor_Contacto** (NVARCHAR(50), NOT NULL): El dato string del contacto (Ej: '0999999999', 'correo@domain.com').
* **Id_Estado_Verificacion** (INT, FK, NOT NULL): Relacionado con `parametro.Tbl_Cat_Item(Id_Item_Catalogo)`.
* **Usuario_Verificador** (NVARCHAR(50), NULL): Asesor u operador que validó el canal de contacto.
* **Fecha_Verificacion** (DATETIME2(3), NULL): Fecha y hora de la validación del canal.
* **Fecha_Creacion** (DATETIME2(3), NOT NULL): Fecha de inserción del registro.
* **Usuario_Creacion** (NVARCHAR(100), NOT NULL): Creador del registro.
* **Esta_Eliminado** (BIT, NOT NULL): Soft-Delete del contacto. `1` = Inactivo/Eliminado.
* **Source** (NVARCHAR(50), NOT NULL): Origen de donde se extrajo el dato (Ej: 'CRM', 'BATCH_REVENUE', 'MOBILE_APP').
* **Id_Estado_LOPDP** (INT, FK, NOT NULL): Relacionado con `parametro.Tbl_Cat_Item(Id_Item_Catalogo)` (Consentimiento de uso).
* **Usuario_Modificacion** (NVARCHAR(100), NULL): Último editor.
* **Fecha_Modificacion** (DATETIME2(3), NULL): Fecha de última edición.
* *Índices y Restricciones Críticas:*
  * `PK_Tbl_Contacto_Cliente` (CLUSTERED) -> `Id_Contacto_Cliente` ASC.
  * `FK_Cliente_Contacto` -> Foreign Key contra `operativo.Tbl_Maest_Cliente(Id_Cliente)`.
  * `FK_Contacto_Cliente_TipoMedioContacto` -> FK contra `parametro.Tbl_Cat_Item(Id_Item_Catalogo)`.
  * `FK_Contacto_Cliente_EstadoVerificado` -> FK contra `parametro.Tbl_Cat_Item(Id_Item_Catalogo)`.
  * `FK_Contacto_Cliente_EstadoLOPDP` -> FK contra `parametro.Tbl_Cat_Item(Id_Item_Catalogo)`.
  * `IX_Contacto_Cliente_Performance_Query` (NONCLUSTERED) -> Claves: `Id_Cliente`, `Id_Estado_Verificacion`, `Id_Tipo_Contacto`. Incluye: `Valor_Contacto`, `Esta_Eliminado`. Condición: `WHERE Esta_Eliminado = 0`. Diseñado para hidratación masiva/lazy loading de perfiles.
  * `IX_Contacto_Cliente_Valor` (NONCLUSTERED) -> Clave: `Valor_Contacto`. Optimiza búsquedas inversas (saber a qué cliente le pertenece un teléfono/correo).
  * `IX_ContactoCliente_IdCliente_EstaEliminado` (NONCLUSTERED) -> Claves: `Id_Cliente`, `Esta_Eliminado`.

### TABLA: operativo.Tbl_Direccion_Cliente (Localización e Información Geográfica)
* **Id_Direccion_Cliente** (INT, PK, IDENTITY(1,1), NOT NULL): Identificador único de la dirección.
* **Id_Cliente** (BIGINT, FK, NOT NULL): Relacionado con `operativo.Tbl_Maest_Cliente(Id_Cliente)`.
* **Id_Tipo_Direccion** (INT, FK, NULL): Relacionado con `parametro.Tbl_Cat_Item(Id_Item_Catalogo)` (Domicilio, Trabajo, etc.).
* **Direccion_Completa** (NVARCHAR(500), NULL): String estructurado de calles, números y referencias de ubicación.
* **Ciudad / Provincia / Pais** (NVARCHAR(100), NULL): Textos descriptivos político-administrativos de la ubicación.
* **Codigo_Pais / Codigo_Ciudad / Codigo_Provincia / Codigo_Parroquia** (NVARCHAR(10), NULL): Códigos ISO/Catastrales/INEC para integraciones API.
* **Parroquia** (NVARCHAR(100), NULL): Nombre de la división política menor de la localidad.
* **Latitud / Longitud** (DECIMAL(18,10), NULL): Coordenadas de geoposicionamiento (compatibles con mapas de alto rendimiento/Leaflet).
* **Es_Principal** (BIT, NULL): `1` = Dirección primaria de contacto del cliente.
* **Esta_Eliminado** (BIT, NOT NULL): Soft-delete de la dirección.
* **Source_Direccion** (NVARCHAR(50), NULL): Canal de origen del dato.
* **Estado_Verificacion** (INT, FK, NULL): Relacionado con `parametro.Tbl_Cat_Item(Id_Item_Catalogo)`.
* **Campos de Auditoría Avanzada** (Fecha/Usuario Creación, Modificación, Verificación y Aprobación utilizando `datetime2(7)` y `nvarchar(50)`).
* **estado_LOPDP** (NVARCHAR(20), NULL): Estado de consentimiento específico para localización (manejo directo en texto).
* *Índices y Restricciones Críticas:*
  * `PK_Tbl_Direccion_Cliente` (CLUSTERED) -> `Id_Direccion_Cliente` ASC.
  * `FK_Cliente_Direccion` -> FK contra `operativo.Tbl_Maest_Cliente(Id_Cliente)`.
  * `FK_Direccion_Cliente_Tipo_Direccion` -> FK contra `parametro.Tbl_Cat_Item(Id_Item_Catalogo)`.
  * `FK_Direccion_Cliente_Estado_verificacion` -> FK contra `parametro.Tbl_Cat_Item(Id_Item_Catalogo)`.
  * `IX_Direccion_Cliente_IdCliente` (NONCLUSTERED) -> Claves: `Id_Direccion_Cliente`, `Es_Principal`. Incluye: `Direccion_Completa`, `Ciudad`, `Latitud`, `Longitud`. Optimiza el renderizado de mapas del cliente principal.

### TABLA: operativo.Tbl_Financiero_cliente (Datos de Capacidad y Perfil Contable)
* **Id_Financiero_Cliente** (BIGINT, PK, IDENTITY(1,1), NOT NULL): Identificador único del perfil financiero.
* **Id_Cliente** (BIGINT, FK, NOT NULL): Relacionado con `operativo.Tbl_Maest_Cliente(Id_Cliente)`.
* **Tipo_Contabilidad** (NVARCHAR(30), NOT NULL): Tipo de régimen (Ej: 'Persona Natural', 'Obligado a llevar Contabilidad', 'Microempresa').
* **Monto_Contable** (DECIMAL(18,2), NOT NULL): Capacidad financiera, ingresos evaluados o cupo contable disponible.
* *Índices y Restricciones Críticas:*
  * `PK_Tbl_Financiero_cliente` (CLUSTERED) -> `Id_Financiero_Cliente` ASC.
  * `FK_Cliente_Financiero` -> FK contra `operativo.Tbl_Maest_Cliente(Id_Cliente)`.
  * `IX_FinancieroCliente_IdCliente` (NONCLUSTERED) -> Clave: `Id_Cliente` ASC. Búsqueda directa uno a uno (1:1 de facto).
  * `IX_FinancieroCliente_TipoContabilidad` (NONCLUSTERED) -> Clave: `Tipo_Contabilidad` ASC. Optimiza reportes de segmentación.

### TABLA: operativo.Tbl_Oficializacion_Core (Auditoría de Sincronización Outbound hacia Core)
* **Id_Oficializacion_Core** (BIGINT, PK, IDENTITY(1,1), NOT NULL): Identificador único del log de oficialización.
* **Id_Cliente** (BIGINT, FK, NOT NULL): Relacionado con `operativo.Tbl_Maest_Cliente(Id_Cliente)`.
* **Trama_Json_Enviada** (NVARCHAR(MAX), NOT NULL): Payload JSON completo enviado al API del Core Bancario (auditoría no repudiable).
* **Respuesta_Core_Codigo** (NVARCHAR(10), NOT NULL): Código de respuesta HTTP o de negocio del Core (Ej: '200', 'ERR045').
* **Fecha_Creacion** (DATETIME2(7), NOT NULL): Estampa exacta de envío y procesamiento.
* **Usuario_Creacion** (NVARCHAR(100), NOT NULL): Usuario o proceso background que disparó la oficialización.
* *Índices y Restricciones Críticas:*
  * `PK_Tbl_Oficializacion_Core` (CLUSTERED) -> `Id_Oficializacion_Core` ASC.
  * `FK_Cliente_Oficializacion` -> FK contra `operativo.Tbl_Maest_Cliente(Id_Cliente)`.
  * `IX_OficializacionCore_IdCliente` (NONCLUSTERED) -> Clave: `Id_Cliente` ASC. Consulta el historial de envíos de un cliente.
  * `IX_OficializacionCore_FechaCreacion` (NONCLUSTERED) -> Clave: `Fecha_Creacion` ASC. Auditoría cronológica de transacciones.

### VISTA: operativo.vw_Contactabilidad_Clientes (Vista Rápida / Segmento Celulares)
* **Objetivo:** Exponer de forma ágil y directa únicamente los números de teléfono celular activos procedentes de campañas o registros rápidos.
* **Campos Expuestos:** `[Identificacion Cliente]`, `[Nombre Completo]`, `Celular`, `Fuente`.
* *Filtro Hardcoded Interno:* `WHERE Con.Id_Tipo_Contacto = 101` (Celular).
* *Lógica de Join:* `operativo.Tbl_Contacto_Cliente Con INNER JOIN operativo.Tbl_Maest_Cliente Mae ON Con.Id_Cliente = Mae.Id_Cliente`.

### VISTA: operativo.vw_Clientes_Contactos_Detalle (Vista Pivot Completa 360 del Cliente)
* **Objetivo:** Aplanar horizontalmente (Pivot) el modelo relacional normalizado para proveer en una sola fila el estado consolidado de contacto del cliente (Celular, Correo, Convencional y Direcciones principales). Ideal para alimentar listados masivos y la caché del backend.
* **Estructura de la Proyección:**
  * Datos Maestro: `[Identificacion Cliente]`, `[Nombre Completo]`.
  * Celular Segmentado: `Celular`, `Fuente_Celular` (Origen: `Id_Tipo_Contacto = 101`).
  * Correo Segmentado: `Correo`, `Fuente_Correo` (Origen: `Id_Tipo_Contacto = 102`).
  * Convencional Segmentado: `Telefono_Convencional`, `Fuente_Telefono_Convencional` (Origen: `Id_Tipo_Contacto = 103`).
  * Dirección Casa: `Direccion_Domicilio`, `Fuente_Direccion_Domicilio` (Origen: `Id_Tipo_Direccion = 105`).
  * Dirección Trabajo: `Direccion_Trabajo`, `Fuente_Direccion_Trabajo` (Origen: `Id_Tipo_Direccion = 106`).
* *Lógica de Join Interna:* Aplica un `FROM operativo.Tbl_Maest_Cliente` seguido de múltiples operaciones `LEFT JOIN operativo.Tbl_Contacto_Cliente` y `LEFT JOIN operativo.Tbl_Direccion_Cliente` filtrados explícitamente por sus códigos ID de catálogo en los predicados de unión (`ON`).

## 1. REGLAS GENERALES Y CONTEXTO DEL ESQUEMA SEGURIDAD
* **Ámbito del Módulo:** Control de acceso e identidad de la plataforma (Identidad de Asesores/Usuarios del sistema).
* **Gestión de Credenciales:** Las contraseñas se almacenan de manera segura bajo algoritmos criptográficos no reversibles en la columna `Clave_Hash`.
* **Manejo de Estados de Bloqueo:** Cuenta con una bandera binaria de protección perimetral (`Esta_Bloqueado`) ante ataques de fuerza bruta.
* **Borrado Lógico:** Controlado por la columna `Esta_Eliminado` (BIT). `1` = Usuario revocado del sistema (Soft-delete), `0` = Usuario vigente en la organización.

## 2. MAPA DE RELACIONES CRÍTICAS (Atajos de Joins para IA)
* `seguridad.Tbl_Maest_Usuario.Id_Usuario` -> Actúa como el identificador de auditoría para las columnas `Usuario_Creacion` y `Usuario_Modificacion` de todas las tablas operativas de la base de datos (vía string o ID según el flujo de aplicación).
* `seguridad.Tbl_Maest_Usuario.Rol_Sistema` <-> Se valida contra las constantes lógicas mapeadas en `parametro.Tbl_Cat_Item.Codigo_Valor` cuando el grupo de catálogo corresponde a los Roles del Sistema.

---

## 3. DEFINICIÓN DE TABLAS

### TABLA: seguridad.Tbl_Maest_Usuario (Maestro de Cuentas y Credenciales de Asesores)
* **Id_Usuario** (INT, PK, IDENTITY(1,1), NOT NULL): Identificador único secuencial e interno de la cuenta del usuario.
* **Codigo_Usuario** (NVARCHAR(50), UNIQUE, NOT NULL): Nombre de usuario único (Username) utilizado como credencial de inicio de sesión en el sistema (Ej: 'jdoe', 'asesor01').
* **Email** (NVARCHAR(100), UNIQUE, NOT NULL): Dirección de correo electrónico corporativo del usuario, obligatoria y única para flujos de recuperación de contraseña o notificaciones de seguridad.
* **Clave_Hash** (NVARCHAR(200), NOT NULL): Cadena de texto que almacena la contraseña encriptada (Password Hash) mediante un algoritmo de derivación de claves seguro (Ej: BCrypt o PBKDF2).
* **Nombre_Asesor** (NVARCHAR(150), NOT NULL): Nombres y apellidos completos del operador o asesor asignado a la cuenta.
* **Rol_Sistema** (NVARCHAR(50), NOT NULL): Texto descriptivo del perfil de permisos asignado (Ej: 'Administrator', 'Supervisor', 'Agent'). Se valida funcionalmente contra la parametrización de catálogos.
* **Fecha_Ultimo_Acceso** (DATETIME2(7), NULL): Marca de tiempo precisa del último inicio de sesión exitoso del usuario en la plataforma.
* **Esta_Activo** (BIT, NOT NULL): Estado operativo de la cuenta. `1` = Cuenta activa con permiso de logueo, `0` = Cuenta suspendida temporalmente.
* **Esta_Eliminado** (BIT, NOT NULL): Flag de Soft-Delete. `1` = Usuario eliminado de la organización, `0` = Usuario activo o latente.
* **Esta_Bloqueado** (BIT, NULL): Control de seguridad de login. `1` = Cuenta bloqueada por exceder el límite de intentos fallidos de contraseña (Lockout).
* **Fecha_Creacion** (DATETIME2(7), NOT NULL): Fecha y hora exacta en la que se dio de alta al usuario en la plataforma.
* **Usuario_Creacion** (NVARCHAR(50), NULL): Identificador del administrador o sistema que registró al usuario.
* **Fecha_Modificacion** (DATETIME2(7), NULL): Fecha de la última alteración de perfil o credenciales.
* **Usuario_Modificacion** (NVARCHAR(50), NULL): Identificador del último usuario que editó este registro.
* *Índices y Restricciones Críticas:*
  * `PK_Tbl_Maest_Usuario` (CLUSTERED) -> Clave: `Id_Usuario` ASC. Organiza físicamente el almacenamiento en disco por el ID secuencial.
  * `IX_Usuario_CodigoUsuario` (UNIQUE NONCLUSTERED) -> Clave: `Codigo_Usuario` ASC. Garantiza de forma estricta que no se dupliquen identificadores de inicio de sesión en el sistema.
  * `IX_Usuario_Email` (UNIQUE NONCLUSTERED) -> Clave: `Email` ASC. Protege la unicidad del correo electrónico para evitar colisiones en la recuperación de cuentas.
  * `IX_Usuario_EstaActivo` (NONCLUSTERED) -> Clave: `Esta_Activo` ASC. Optimiza las consultas del motor que listan o validan únicamente al personal disponible para la operación en tiempo real.