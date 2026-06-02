# Diccionario de Datos - Plataforma de Gestión de Datos (PGD)

Motor de Base de Datos: SQL Server (Compatibility Level 150)
Zona Horaria: Todos los campos datetime2 se almacenan con la hora local del servidor (sin zona horaria explícita).
Nomenclatura: Esquemas y tablas en PascalCase con prefijo Tbl_ o Vw_. Columnas en PascalCase separados por guion bajo (Snake_Pascal_Case).
Borrado Lógico: Controlado por la columna Esta_Eliminado (BIT). 1 = Eliminado lógicamente, 0 = Activo. Excepto en el esquema parametro, donde se usa Esta_Activo como indicador de estado.

Esquemas disponibles: parametro, operativo, seguridad, carga.

---

## ESQUEMA: parametro

Propósito: Catálogos configurables y constantes de sistema. Actúa como tabla de verdad para los valores de los combos, tipos y estados usados por los esquemas operativo y seguridad.

### Mapa de Relaciones Críticas

* parametro.Tbl_Cat_Grupo.Id_Grupo_Catalogo <-> parametro.Tbl_Cat_Item.Id_Grupo_Catalogo (Relación Maestro-Detalle. Un grupo agrupa N ítems o constantes).
* parametro.Tbl_Cat_Item.Id_Item_Catalogo <-> operativo.Tbl_Maest_Cliente.Id_Tipo_Identificacion (Mapea el tipo de documento del cliente).
* parametro.Tbl_Cat_Item.Id_Item_Catalogo <-> operativo.Tbl_Contacto_Cliente.Id_Tipo_Contacto (Define si es Celular, Correo, Convencional, etc.).
* parametro.Tbl_Cat_Item.Id_Item_Catalogo <-> operativo.Tbl_Contacto_Cliente.Id_Estado_Verificacion (Estado del medio de contacto).

---

### TABLA: parametro.Tbl_Cat_Grupo (Agrupadores de catálogos y constantes de sistema)

* Id_Grupo_Catalogo (INT, PK, IDENTITY(1,1), NOT NULL): Identificador único auto-incrementable del grupo.
* Nombre_Grupo (NVARCHAR(100), UNIQUE, NOT NULL): Nombre único usado por los Enums de la aplicación para mapear constantes.
* Descripcion (NVARCHAR(250), NULL): Explicación funcional del propósito del grupo.
* Es_Sistema (BIT, DEFAULT 0, NOT NULL): 1 = Crítico para el software, no modificable por usuario. 0 = Modificable.
* Fecha_Creacion (DATETIME2(3), DEFAULT getdate(), NOT NULL): Fecha de registro del grupo.
* Usuario_Creacion (NVARCHAR(50), NULL): Usuario que registró el grupo.
* Fecha_Modificacion (DATETIME2(3), NULL): Fecha de última actualización.
* Usuario_Modificacion (NVARCHAR(50), NULL): Usuario que realizó la última actualización.

Índices:
* PK_Tbl_Cat_Grupo (CLUSTERED) -> Id_Grupo_Catalogo ASC.
* UQ_GrupoCatalogo_NombreGrupo (UNIQUE NONCLUSTERED) -> Nombre_Grupo ASC.

---

### TABLA: parametro.Tbl_Cat_Item (Valores y opciones específicas por cada grupo)

* Id_Item_Catalogo (INT, PK, NOT NULL): Identificador único global y fijo del ítem. Clave foránea destino de tablas operativas. No usa IDENTITY — el valor se asigna deliberadamente para garantizar estabilidad de las referencias entre esquemas.
* Id_Grupo_Catalogo (INT, FK, NOT NULL): Relacionado con parametro.Tbl_Cat_Grupo(Id_Grupo_Catalogo).
* Codigo_Valor (NVARCHAR(20), NOT NULL): Código alfanumérico corto usado en lógica de backend (Ej: 'CED', 'ADMIN').
* Texto_Visual (NVARCHAR(100), NOT NULL): Etiqueta en lenguaje natural expuesta en los combos de la interfaz.
* Orden_Visual (INT, NOT NULL): Peso numérico que define el orden de aparición en la interfaz.
* Esta_Activo (BIT, DEFAULT 1, NOT NULL): 1 = Disponible para uso. 0 = Deshabilitado (baja lógica).
* Fecha_Creacion (DATETIME2(3), DEFAULT getdate(), NOT NULL): Fecha de registro del ítem.
* Usuario_Creacion (NVARCHAR(50), NULL): Usuario que registró el ítem.
* Fecha_Modificacion (DATETIME2(3), NULL): Fecha de última modificación.
* Usuario_Modificacion (NVARCHAR(50), NULL): Usuario que modificó el ítem.

Índices y Restricciones:
* PK_Tbl_Cat_Item (CLUSTERED) -> Id_Item_Catalogo ASC.
* FK_Grupo_Item -> Foreign Key contra parametro.Tbl_Cat_Grupo(Id_Grupo_Catalogo).
* UQ_ItemCatalogo_IdGrupoCatalogo_CodigoValor (UNIQUE NONCLUSTERED) -> Evita códigos duplicados dentro de un mismo grupo.
* IX_ItemCatalogo_Buscador (NONCLUSTERED) -> Claves: Id_Grupo_Catalogo ASC, Esta_Activo ASC. Incluye: Codigo_Valor, Texto_Visual, Orden_Visual. Optimiza la carga de combos desde el backend.

---

### VISTA: parametro.Vw_Cat_Detalle_General (Query Object con SCHEMABINDING — IMPLEMENTADA)

Expone en una sola proyección los ítems junto con el nombre y flag de protección de su grupo padre. Usada por el backend para hidratar combos sin joins manuales.

* Id_Item_Catalogo (INT): Identificador del ítem.
* Id_Grupo_Catalogo (INT): Identificador del grupo.
* Nombre_Grupo (NVARCHAR(100)): Nombre del grupo vinculante.
* Codigo_Valor (NVARCHAR(20)): Código funcional del ítem.
* Texto_Visual (NVARCHAR(100)): Texto de interfaz.
* Orden_Visual (INT): Secuencia en combos.
* Esta_Activo (BIT): Estado de disponibilidad.
* Es_Sistema (BIT): Flag de protección del grupo.

Join interno: parametro.Tbl_Cat_Item i INNER JOIN parametro.Tbl_Cat_Grupo g ON i.Id_Grupo_Catalogo = g.Id_Grupo_Catalogo.

---

## ESQUEMA: operativo

Propósito: Centralización del perfil 360 del cliente (identificación, contactos, ubicación y capacidad financiera) junto con la auditoría de integración con el Core Bancario.

Llaves Primarias: BIGINT IDENTITY(1,1) para el maestro de clientes por volumen transaccional. Las tablas de detalle usan INT o BIGINT IDENTITY(1,1) según proyección de crecimiento.
Borrado Lógico: Columna Esta_Eliminado (BIT). 1 = Eliminado lógicamente, excluir de consultas de negocio. 0 = Activo.
Privacidad: Incluye banderas para control de anonimización bajo normativas LOPDP.

### Mapa de Relaciones Críticas

* operativo.Tbl_Maest_Cliente.Id_Cliente <-> operativo.Tbl_Contacto_Cliente.Id_Cliente (Un cliente tiene muchos medios de contacto: celular, correo, convencional).
* operativo.Tbl_Maest_Cliente.Id_Cliente <-> operativo.Tbl_Direccion_Cliente.Id_Cliente (Un cliente tiene muchas direcciones físicas registradas).
* operativo.Tbl_Maest_Cliente.Id_Cliente <-> operativo.Tbl_Financiero_cliente.Id_Cliente (Un cliente tiene un perfil de segmentación contable).
* operativo.Tbl_Maest_Cliente.Id_Cliente <-> operativo.Tbl_Oficializacion_Core.Id_Cliente (Historial de sincronización del cliente hacia sistemas Core externos).
* parametro.Tbl_Cat_Item.Id_Item_Catalogo <-> operativo.Tbl_Maest_Cliente.Id_Tipo_Identificacion.
* parametro.Tbl_Cat_Item.Id_Item_Catalogo <-> operativo.Tbl_Contacto_Cliente.Id_Tipo_Contacto.
* parametro.Tbl_Cat_Item.Id_Item_Catalogo <-> operativo.Tbl_Contacto_Cliente.Id_Estado_Verificacion.

---

### TABLA: operativo.Tbl_Maest_Cliente (Entidad Maestra / Aggregate Root de Clientes)

* Id_Cliente (BIGINT, PK, IDENTITY(1,1), NOT NULL): Identificador único global e interno del cliente.
* Identificacion_Cliente (NVARCHAR(20), NOT NULL): Número de documento de identidad (Cédula, RUC, Pasaporte).
* Id_Tipo_Identificacion (INT, FK, NOT NULL): Relacionado con parametro.Tbl_Cat_Item(Id_Item_Catalogo).
* Nombre_Completo (NVARCHAR(250), NULL): Nombres y apellidos completos del cliente.
* Esta_Verificado (BIT, NOT NULL): Estado de validación biométrica o de buró del cliente.
* Esta_Aprobado (BIT, NOT NULL): Estado transaccional/comercial para operar en la plataforma.
* Esta_Eliminado (BIT, NOT NULL): Soft-Delete. 1 = Eliminado. 0 = Activo.
* Esta_Anonimizado (BIT, NOT NULL): 1 = Datos personales ofuscados por derecho al olvido (LOPDP).
* Fecha_Creacion (DATETIME2(3), NOT NULL): Fecha de registro inicial en el sistema.
* Usuario_Creacion (NVARCHAR(100), NOT NULL): Operador, asesor o sistema que registró al cliente.
* Fecha_Modificacion (DATETIME2(3), NULL): Fecha de última actualización del perfil maestro.
* Usuario_Modificacion (NVARCHAR(100), NULL): Último usuario que editó el registro.
* Fecha_Expiracion_Legal (DATE, NULL): Fecha de caducidad del documento o vigencia legal del cliente.

Índices y Restricciones:
* PK_Tbl_Maest_Cliente (CLUSTERED) -> Id_Cliente ASC.
* FK_Cliente_TipoIdentificacion -> Foreign Key contra parametro.Tbl_Cat_Item(Id_Item_Catalogo).
* IX_Cliente_Identificacion (UNIQUE NONCLUSTERED, FILTRADO) -> Clave: Identificacion_Cliente. Filtro: WHERE Esta_Eliminado = 0 AND Esta_Anonimizado = 0. Garantiza unicidad operativa sin afectar registros eliminados/anonimizados.
* IX_Cliente_Nombre (NONCLUSTERED, FILTRADO) -> Clave: Nombre_Completo. Filtro: WHERE Esta_Eliminado = 0 AND Esta_Anonimizado = 0. Optimiza la búsqueda predictiva por texto.

---

### TABLA: operativo.Tbl_Contacto_Cliente (Medios de Contactabilidad)

* Id_Contacto_Cliente (INT, PK, IDENTITY(1,1), NOT NULL): Identificador único del medio de contacto.
* Id_Cliente (BIGINT, FK, NOT NULL): Relacionado con operativo.Tbl_Maest_Cliente(Id_Cliente).
* Id_Tipo_Contacto (INT, FK, NOT NULL): Relacionado con parametro.Tbl_Cat_Item(Id_Item_Catalogo). Define si es Celular, Correo, Convencional, etc.
* Valor_Contacto (NVARCHAR(50), NOT NULL): El dato string del contacto (Ej: '0999999999', 'correo@dominio.com').
* Id_Estado_Verificacion (INT, FK, NOT NULL): Relacionado con parametro.Tbl_Cat_Item(Id_Item_Catalogo). Estado de validez del canal.
* Usuario_Verificador (NVARCHAR(50), NULL): Asesor u operador que validó el canal de contacto.
* Fecha_Verificacion (DATETIME2(3), NULL): Fecha y hora de la validación del canal.
* Fecha_Creacion (DATETIME2(3), NOT NULL): Fecha de inserción del registro.
* Usuario_Creacion (NVARCHAR(100), NOT NULL): Creador del registro.
* Esta_Eliminado (BIT, NOT NULL): Soft-Delete del contacto. 1 = Inactivo/Eliminado.
* Source (NVARCHAR(50), NOT NULL): Origen del dato (Ej: 'CRM', 'BATCH_REVENUE', 'MOBILE_APP'). Nombre en inglés por convención heredada.
* Id_Estado_LOPDP (INT, FK, NOT NULL): Relacionado con parametro.Tbl_Cat_Item(Id_Item_Catalogo). Consentimiento de uso del dato bajo normativa LOPDP.
* Usuario_Modificacion (NVARCHAR(100), NULL): Último editor del registro.
* Fecha_Modificacion (DATETIME2(3), NULL): Fecha de última edición.

Índices y Restricciones:
* PK_Tbl_Contacto_Cliente (CLUSTERED) -> Id_Contacto_Cliente ASC.
* FK_Cliente_Contacto -> Foreign Key contra operativo.Tbl_Maest_Cliente(Id_Cliente).
* FK_Contacto_Cliente_TipoMedioContacto -> FK contra parametro.Tbl_Cat_Item(Id_Item_Catalogo).
* FK_Contacto_Cliente_EstadoVerificado -> FK contra parametro.Tbl_Cat_Item(Id_Item_Catalogo).
* FK_Contacto_Cliente_EstadoLOPDP -> FK contra parametro.Tbl_Cat_Item(Id_Item_Catalogo).
* IX_Contacto_Cliente_Performance_Query (NONCLUSTERED, FILTRADO) -> Claves: Id_Cliente, Id_Estado_Verificacion, Id_Tipo_Contacto. Incluye: Valor_Contacto, Esta_Eliminado. Filtro: WHERE Esta_Eliminado = 0. Diseñado para hidratación masiva de perfiles de contacto.
* IX_Contacto_Cliente_Valor (NONCLUSTERED) -> Clave: Valor_Contacto. Optimiza búsquedas inversas (identificar a qué cliente pertenece un número o correo).
* IX_ContactoCliente_IdCliente_EstaEliminado (NONCLUSTERED) -> Claves: Id_Cliente, Esta_Eliminado.

---

### TABLA: operativo.Tbl_Direccion_Cliente (Localización e Información Geográfica)

* Id_Direccion_Cliente (INT, PK, IDENTITY(1,1), NOT NULL): Identificador único de la dirección.
* Id_Cliente (BIGINT, FK, NOT NULL): Relacionado con operativo.Tbl_Maest_Cliente(Id_Cliente).
* Id_Tipo_Direccion (INT, FK, NULL): Relacionado con parametro.Tbl_Cat_Item(Id_Item_Catalogo). (Domicilio, Trabajo, etc.).
* Direccion_Completa (NVARCHAR(500), NULL): String estructurado de calles, números y referencias de ubicación.
* Ciudad (NVARCHAR(100), NULL): Texto descriptivo de la ciudad.
* Provincia (NVARCHAR(100), NULL): Texto descriptivo de la provincia.
* Pais (NVARCHAR(100), NULL): Texto descriptivo del país.
* Codigo_Pais (NVARCHAR(10), NULL): Código ISO del país para integraciones API.
* Codigo_Ciudad (NVARCHAR(10), NULL): Código catastral/INEC de la ciudad.
* Codigo_Provincia (NVARCHAR(10), NULL): Código catastral/INEC de la provincia.
* Codigo_Parroquia (NVARCHAR(10), NULL): Código catastral/INEC de la parroquia.
* Parroquia (NVARCHAR(100), NULL): Nombre de la división política menor de la localidad.
* Latitud (DECIMAL(18,10), NULL): Coordenada de latitud (compatible con Leaflet y APIs de mapas).
* Longitud (DECIMAL(18,10), NULL): Coordenada de longitud.
* Es_Principal (BIT, NULL): 1 = Dirección primaria de contacto del cliente.
* Esta_Eliminado (BIT, NOT NULL): Soft-Delete de la dirección.
* Source_Direccion (NVARCHAR(50), NULL): Canal de origen del dato.
* Estado_Verificacion (INT, FK, NULL): Relacionado con parametro.Tbl_Cat_Item(Id_Item_Catalogo).
* estado_LOPDP (NVARCHAR(20), NULL): Estado de consentimiento para localización. Almacenado como texto libre (no FK a catálogo). Nombre en minúsculas por inconsistencia de origen — pendiente de corrección.
* Campos de Auditoría: Fecha_Creacion, Usuario_Creacion, Fecha_Modificacion, Usuario_Modificacion, Fecha_Verificacion, Usuario_Verificador, Fecha_Aprobacion, Usuario_Aprobacion (datetime2(7) y nvarchar(50)).

Índices y Restricciones:
* PK_Tbl_Direccion_Cliente (CLUSTERED) -> Id_Direccion_Cliente ASC.
* FK_Cliente_Direccion -> FK contra operativo.Tbl_Maest_Cliente(Id_Cliente).
* FK_Direccion_Cliente_Tipo_Direccion -> FK contra parametro.Tbl_Cat_Item(Id_Item_Catalogo).
* FK_Direccion_Cliente_Estado_verificacion -> FK contra parametro.Tbl_Cat_Item(Id_Item_Catalogo).
* IX_Direccion_Cliente_IdCliente (NONCLUSTERED) -> Claves: Id_Direccion_Cliente, Es_Principal. Incluye: Direccion_Completa, Ciudad, Latitud, Longitud. Optimiza el renderizado del mapa de la dirección principal del cliente.

---

### TABLA: operativo.Tbl_Financiero_cliente (Datos de Capacidad y Perfil Contable)

* Id_Financiero_Cliente (BIGINT, PK, IDENTITY(1,1), NOT NULL): Identificador único del perfil financiero.
* Id_Cliente (BIGINT, FK, NOT NULL): Relacionado con operativo.Tbl_Maest_Cliente(Id_Cliente).
* Tipo_Contabilidad (NVARCHAR(30), NOT NULL): Tipo de régimen (Ej: 'Persona Natural', 'Obligado a llevar Contabilidad', 'Microempresa').
* Monto_Contable (DECIMAL(18,2), NOT NULL): Capacidad financiera, ingresos evaluados o cupo contable disponible.

Índices y Restricciones:
* PK_Tbl_Financiero_cliente (CLUSTERED) -> Id_Financiero_Cliente ASC.
* FK_Cliente_Financiero -> FK contra operativo.Tbl_Maest_Cliente(Id_Cliente).
* IX_FinancieroCliente_IdCliente (NONCLUSTERED) -> Clave: Id_Cliente ASC. Búsqueda directa 1:1 de facto.
* IX_FinancieroCliente_TipoContabilidad (NONCLUSTERED) -> Clave: Tipo_Contabilidad ASC. Optimiza reportes de segmentación.

---

### TABLA: operativo.Tbl_Oficializacion_Core (Auditoría de Sincronización Outbound hacia Core)

* Id_Oficializacion_Core (BIGINT, PK, IDENTITY(1,1), NOT NULL): Identificador único del log de oficialización.
* Id_Cliente (BIGINT, FK, NOT NULL): Relacionado con operativo.Tbl_Maest_Cliente(Id_Cliente).
* Trama_Json_Enviada (NVARCHAR(MAX), NOT NULL): Payload JSON completo enviado al API del Core Bancario (auditoría no repudiable).
* Respuesta_Core_Codigo (NVARCHAR(10), NOT NULL): Código de respuesta HTTP o de negocio del Core (Ej: '200', 'ERR045').
* Fecha_Creacion (DATETIME2(7), NOT NULL): Estampa exacta de envío y procesamiento.
* Usuario_Creacion (NVARCHAR(100), NOT NULL): Usuario o proceso en background que disparó la oficialización.

Índices y Restricciones:
* PK_Tbl_Oficializacion_Core (CLUSTERED) -> Id_Oficializacion_Core ASC.
* FK_Cliente_Oficializacion -> FK contra operativo.Tbl_Maest_Cliente(Id_Cliente).
* IX_OficializacionCore_IdCliente (NONCLUSTERED) -> Clave: Id_Cliente ASC. Consulta el historial de envíos de un cliente.
* IX_OficializacionCore_FechaCreacion (NONCLUSTERED) -> Clave: Fecha_Creacion ASC. Auditoría cronológica de transacciones.

---

### VISTA: operativo.vw_Contactabilidad_Clientes — PENDIENTE DE IMPLEMENTACIÓN

Propósito diseñado: Exponer de forma ágil únicamente los números de teléfono celular activos procedentes de campañas o registros rápidos.
Campos proyectados: Identificacion_Cliente, Nombre_Completo, Celular, Fuente.
Filtro interno: WHERE Id_Tipo_Contacto = [Id del ítem Celular en parametro.Tbl_Cat_Item].
Join: operativo.Tbl_Contacto_Cliente INNER JOIN operativo.Tbl_Maest_Cliente ON Id_Cliente.
Estado: No implementada en scripts SQL ni en migraciones EF Core.

---

### VISTA: operativo.vw_Clientes_Contactos_Detalle — PENDIENTE DE IMPLEMENTACIÓN

Propósito diseñado: Aplanar horizontalmente (Pivot) el modelo relacional normalizado para proveer en una sola fila el estado consolidado de contacto del cliente. Ideal para listados masivos y caché del backend.
Proyección diseñada:
* Datos maestro: Identificacion_Cliente, Nombre_Completo.
* Celular: Celular, Fuente_Celular (Id_Tipo_Contacto del ítem Celular).
* Correo: Correo, Fuente_Correo (Id_Tipo_Contacto del ítem Correo).
* Convencional: Telefono_Convencional, Fuente_Telefono_Convencional (Id_Tipo_Contacto del ítem Convencional).
* Dirección domicilio: Direccion_Domicilio, Fuente_Direccion_Domicilio (Id_Tipo_Direccion del ítem Domicilio).
* Dirección trabajo: Direccion_Trabajo, Fuente_Direccion_Trabajo (Id_Tipo_Direccion del ítem Trabajo).
Join diseñado: FROM operativo.Tbl_Maest_Cliente con múltiples LEFT JOIN a Tbl_Contacto_Cliente y Tbl_Direccion_Cliente filtrados por el Id_Item_Catalogo correspondiente en el predicado ON.
Estado: No implementada en scripts SQL ni en migraciones EF Core.

---

## ESQUEMA: seguridad

Propósito: Control de acceso e identidad de la plataforma. Gestiona las cuentas de los usuarios internos del sistema (asesores, supervisores, administradores).

Gestión de Credenciales: Las contraseñas se almacenan como hash no reversible en la columna Clave_Hash.
Protección contra fuerza bruta: Bandera Esta_Bloqueado activa el lockout de la cuenta al superar el límite de intentos fallidos.
Borrado Lógico: Columna Esta_Eliminado (BIT). 1 = Usuario revocado. 0 = Usuario vigente.

### Mapa de Relaciones Críticas

* seguridad.Tbl_Maest_Usuario.Id_Usuario: Actúa como referencia de auditoría para las columnas Usuario_Creacion y Usuario_Modificacion de todas las tablas operativas (vía string o ID según el flujo de la aplicación).
* seguridad.Tbl_Maest_Usuario.Rol_Sistema: Se valida funcionalmente contra parametro.Tbl_Cat_Item.Codigo_Valor del grupo de catálogo correspondiente a Roles del Sistema.

---

### TABLA: seguridad.Tbl_Maest_Usuario (Maestro de Cuentas y Credenciales de Usuarios)

* Id_Usuario (INT, PK, IDENTITY(1,1), NOT NULL): Identificador único secuencial e interno de la cuenta.
* Codigo_Usuario (NVARCHAR(50), UNIQUE, NOT NULL): Nombre de usuario (Username) usado como credencial de inicio de sesión (Ej: 'jdoe', 'asesor01').
* Email (NVARCHAR(100), UNIQUE, NOT NULL): Correo corporativo del usuario. Único y obligatorio para flujos de recuperación de contraseña.
* Clave_Hash (NVARCHAR(200), NOT NULL): Hash de la contraseña mediante algoritmo de derivación seguro (BCrypt o PBKDF2).
* Nombre_Asesor (NVARCHAR(150), NOT NULL): Nombres y apellidos completos del operador asignado a la cuenta.
* Rol_Sistema (NVARCHAR(50), NOT NULL): Perfil de permisos asignado (Ej: 'Administrator', 'Supervisor', 'Agent'). Se valida contra la parametrización de catálogos.
* Fecha_Ultimo_Acceso (DATETIME2(7), NULL): Marca de tiempo del último inicio de sesión exitoso.
* Esta_Activo (BIT, NOT NULL): 1 = Cuenta activa con permiso de login. 0 = Cuenta suspendida temporalmente.
* Esta_Eliminado (BIT, NOT NULL): Soft-Delete. 1 = Usuario eliminado de la organización. 0 = Usuario activo o latente.
* Esta_Bloqueado (BIT, NULL): 1 = Cuenta bloqueada por exceder el límite de intentos fallidos (Lockout).
* Fecha_Creacion (DATETIME2(7), NOT NULL): Fecha y hora de alta del usuario en la plataforma.
* Usuario_Creacion (NVARCHAR(50), NULL): Administrador o sistema que registró al usuario.
* Fecha_Modificacion (DATETIME2(7), NULL): Fecha de la última alteración de perfil o credenciales.
* Usuario_Modificacion (NVARCHAR(50), NULL): Último usuario que editó este registro.

Índices y Restricciones:
* PK_Tbl_Maest_Usuario (CLUSTERED) -> Id_Usuario ASC.
* IX_Usuario_CodigoUsuario (UNIQUE NONCLUSTERED) -> Clave: Codigo_Usuario ASC. Garantiza que no se dupliquen identificadores de login.
* IX_Usuario_Email (UNIQUE NONCLUSTERED) -> Clave: Email ASC. Protege la unicidad del correo para evitar colisiones en la recuperación de cuentas.
* IX_Usuario_EstaActivo (NONCLUSTERED) -> Clave: Esta_Activo ASC. Optimiza las consultas que listan o validan únicamente al personal disponible en tiempo real.

---

## ESQUEMA: carga — PENDIENTE DE IMPLEMENTACIÓN

Propósito diseñado: Zona de staging para la ingesta de archivos de datos provistos por proveedores externos autorizados (Ej: bureaus de crédito). Actúa como buffer de validación previo a la promoción de los datos al esquema operativo.

Estado: No implementado. No existen tablas, migraciones, configuraciones EF Core ni scripts SQL asociados a este esquema. La responsabilidad de su diseño e implementación recae en el equipo técnico cuando se formalice el contrato con el primer proveedor de datos externo.
