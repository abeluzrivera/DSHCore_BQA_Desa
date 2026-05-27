# Procedimientos Operativos — Plataforma Data Smart Hub (DSH)

**Versión:** 1.2  
**Fecha de emisión:** 27 de mayo de 2026  
**Clasificación:** Uso interno restringido

---

## 1. Objetivo

Establecer los lineamientos, políticas y procedimientos operativos que regulan el uso, administración y gestión de la plataforma **Data Smart Hub (DSH)**, a fin de asegurar la correcta localización de clientes que no han podido ser contactados mediante los canales convencionales, garantizando la integridad de los datos, la seguridad de acceso y la trazabilidad de las operaciones.

---

## 2. Alcance

El presente documento aplica a todos los colaboradores, áreas y departamentos que interactúan con la plataforma Data Smart Hub, incluyendo:

- Area de Negocios
- Area de Recuperación de Créditos
- Area de Atención al Cliente
- Departamento de Gestión de Datos
- Area de Seguridad Bancaria

La plataforma se encuentra publicada en la red interna institucional y es accesible exclusivamente por personal interno debidamente autorizado.

---

## 3. Marco de Políticas

### 3.1 Políticas Generales

1. La plataforma DSH es un sistema de uso exclusivo para fines institucionales orientados a la gestión de contactabilidad de clientes.
2. El acceso a la plataforma se realiza mediante las credenciales del Active Directory corporativo. Queda prohibido el uso de cuentas de usuario ajenas a las propias.
3. La información gestionada en la plataforma tiene carácter confidencial y está sujeta a la normativa de protección de datos vigente.
4. Cualquier incidente de seguridad, acceso no autorizado o comportamiento anómalo debe ser reportado de forma inmediata al Área de Seguridad Bancaria y al administrador de la plataforma.
5. Los datos incorporados a la plataforma deben provenir exclusivamente de fuentes oficiales autorizadas por la institución.
6. Los registros y trazas de auditoría generados por la plataforma no podrán ser eliminados ni modificados bajo ninguna circunstancia.

### 3.2 Políticas Específicas

#### 3.2.1 Gestión de Acceso y Seguridad

1. El acceso a la plataforma DSH se concede mediante la asignación del colaborador a un grupo de seguridad del Active Directory corporativo. No se crean cuentas locales en la plataforma.
2. La habilitación de acceso requiere la aprobación conjunta del Subgerente del Área solicitante y del Subgerente de Gestión de Datos. Una vez otorgada dicha aprobación, la ejecución del proceso de asignación es responsabilidad exclusiva del Área de Seguridad Bancaria, conforme a su proceso normado.
3. Los perfiles de acceso se asignarán conforme al principio de mínimo privilegio, otorgando únicamente los permisos estrictamente necesarios para el desempeño de las funciones del colaborador.
4. Se debe mantener un registro actualizado de los grupos de seguridad a los que pertenece cada usuario, a fin de garantizar que ante un cese de actividades, traslado o cambio de funciones, el colaborador sea removido de todos los grupos que le otorgan acceso a la plataforma, sin excepción.
5. Los grupos de seguridad disponibles en la plataforma y sus niveles de acceso son los siguientes:

   | Grupo de seguridad | Descripción de acceso |
   |---|---|
   | `DSH_ADMIN` | Acceso de administración. Configuraciones, monitoreo y datos maestros de la plataforma. |
   | `DSH_SUPERVISOR` | Supervisión operativa. Acceso a validaciones y seguimiento del equipo. |
   | `DSH_AGENTE` | Operativa de contactabilidad. Consulta y registro de gestiones de contacto. |
   | `DSH_CONSULTA` | Acceso de solo lectura sobre información de clientes. |

6. Ningún usuario perteneciente a los grupos `DSH_AGENTE` o `DSH_CONSULTA` podrá crear, modificar ni eliminar registros fuera del ámbito de su gestión operativa.

#### 3.2.2 Gestión de Datos

1. La carga de datos provenientes de proveedores externos se ejecuta a demanda, únicamente cuando se encuentre vigente un contrato con el proveedor correspondiente y este haya enviado formalmente los archivos de datos. No constituye una actividad periódica o programada.
2. La operativa diaria del Departamento de Gestión de Datos comprende la revisión y validación de los clientes que hayan sido confirmados durante la jornada, cruzando la información de la plataforma con la disponible en el core institucional.
3. Toda carga masiva de datos debe ser validada contra la estructura del esquema de datos de la plataforma antes de su oficialización.
4. Todo proceso de reparación o enriquecimiento de datos requiere una validación cruzada con la información contenida en el core institucional, así como el estricto cumplimiento de la normativa procedimental y regulatoria vigente. La oficialización de cualquier dato estará condicionada a la superación de esta confrontación técnica y a su total conformidad con el marco legal aplicable en cada momento.
5. Queda prohibida la modificación manual de datos en la base de datos de la plataforma sin dejar registro de auditoría.

#### 3.2.3 Operativa de Contactabilidad

1. La consulta de información de un cliente en la plataforma debe realizarse únicamente en el contexto de una gestión operativa activa y debidamente justificada.
2. El registro de resultados de contacto (exitoso o fallido) es obligatorio una vez iniciada la gestión sobre un cliente.
3. Los filtros de búsqueda disponibles en la plataforma deben ser utilizados para acotar las consultas y evitar exposiciones innecesarias de información.

---

## 4. Procedimientos Operativos

### 4.1 Procedimiento de Carga de Datos de Proveedores Externos

**Responsable:** Personal designado del Departamento de Gestión de Datos  
**Frecuencia:** A demanda, cuando el proveedor remita los archivos contratados

#### Pasos

1. El Departamento de Gestión de Datos recibe la notificación formal del proveedor externo autorizado (por ejemplo, Equifax) indicando la disponibilidad de los archivos de datos.
2. Verifica que existe un contrato vigente con dicho proveedor y que el envío corresponde al alcance contratado.
3. Verifica la integridad y formato de los archivos recibidos conforme a la especificación técnica vigente.
4. Carga los datos al esquema de staging de la plataforma DSH mediante el mecanismo de carga masiva habilitado.
5. Ejecuta el proceso de transformación y adaptación de los datos al esquema operativo de la plataforma, validando que la estructura y los tipos de datos sean correctos.
6. Informa al supervisor de su departamento sobre el resultado de la carga, indicando el volumen procesado y cualquier incidencia detectada.

---

### 4.2 Procedimiento de Gestión Diaria de Datos

**Responsable:** Personal designado del Departamento de Gestión de Datos  
**Frecuencia:** Diaria

#### Pasos

1. Al inicio de la jornada, el responsable de Gestión de Datos accede a la plataforma DSH para revisar los clientes que hayan sido confirmados durante la jornada anterior.
2. Para cada cliente confirmado, procede a la validación cruzada entre la información de contactabilidad registrada en la plataforma y la información disponible en el core institucional.
3. Determina si los datos requieren reparación o enriquecimiento y ejecuta las acciones correspondientes dentro de la plataforma.
4. Registra el resultado de cada validación, indicando si los datos fueron confirmados, reparados o descartados.
5. Escala al supervisor de su departamento cualquier caso que no pueda ser resuelto en el marco de la operativa estándar.

---

### 4.3 Procedimiento de Gestión de Acceso a la Plataforma

**Responsable principal:** Área de Seguridad Bancaria  
**Autorización previa:** Subgerente del Área solicitante y Subgerente de Gestión de Datos  
**Disparador:** Solicitud formal de habilitación, modificación o revocación de acceso

#### 4.3.1 Habilitación de Acceso (Asignación a Grupo de Seguridad)

1. El Subgerente del Área solicitante eleva una solicitud formal de acceso a la plataforma DSH, especificando nombre completo del colaborador, cuenta de dominio corporativo, área o departamento y grupo de seguridad requerido.
2. El Subgerente de Gestión de Datos revisa y co-aprueba la solicitud, validando que el nivel de acceso requerido es pertinente para las funciones del colaborador.
3. El administrador de la plataforma DSH recibe la solicitud aprobada y verifica que el acceso solicitado es consistente con el rol del colaborador y con las políticas de la plataforma. Su participación en esta etapa es de validación y aprobación técnica, no de ejecución.
4. Con la aprobación de ambos subgerentes confirmada, el Área de Seguridad Bancaria ejecuta la asignación del colaborador al grupo de seguridad correspondiente en el Active Directory corporativo, conforme a su proceso normado.
5. El colaborador puede acceder a la plataforma DSH utilizando sus credenciales de dominio corporativo desde la red interna institucional, sin requerir credenciales adicionales.
6. El Área de Seguridad Bancaria registra la asignación efectuada, incluyendo fecha, grupo asignado y aprobaciones obtenidas.

#### 4.3.2 Modificación de Grupo de Seguridad

1. El Subgerente del Área solicitante eleva la solicitud de cambio de grupo, indicando el grupo actual y el grupo al que debe ser migrado el colaborador, con justificación del motivo.
2. El Subgerente de Gestión de Datos co-aprueba la solicitud.
3. El administrador de la plataforma valida que el nuevo nivel de acceso es pertinente.
4. El Área de Seguridad Bancaria ejecuta el cambio de grupo en el Active Directory y registra la modificación con fecha y motivo.
5. Se notifica al colaborador y al Subgerente del Área solicitante de la efectividad del cambio.

#### 4.3.3 Revocación de Acceso (Cese, Traslado o Suspensión)

1. Ante el cese de actividades, traslado o suspensión de un colaborador, el Subgerente del Área notifica de forma inmediata al Área de Seguridad Bancaria y al administrador de la plataforma.
2. El Área de Seguridad Bancaria retira al colaborador de todos los grupos de seguridad del Active Directory que le otorgan acceso a la plataforma DSH, dentro de las cuatro (4) horas hábiles siguientes a la notificación. En situaciones de desvinculación inmediata o riesgo, la revocación debe ejecutarse sin demora.
3. El Área de Seguridad Bancaria verifica en su registro de asignaciones que el colaborador ha sido removido de la totalidad de grupos vinculados a la plataforma, sin excepción.
4. Se deja constancia de la revocación con fecha, hora, grupos afectados y motivo.

---

### 4.4 Procedimiento de Operativa de Contactabilidad

**Responsable:** Usuario operativo (grupo `DSH_AGENTE`) supervisado por un usuario del grupo `DSH_SUPERVISOR`  
**Frecuencia:** Continua durante la jornada operativa

#### Pasos

1. El colaborador autorizado inicia sesión en la plataforma Data Smart Hub desde la red interna institucional, utilizando sus credenciales del Active Directory corporativo.
2. En el marco de su operativa oficial, cuando debe gestionar el contacto con un cliente no localizado, consulta en la plataforma si el cliente dispone de información de contacto verificada.
3. Si el cliente cuenta con información verificada y vigente, utiliza dicha información para proceder con el contacto por los canales habilitados.
4. Si el cliente no cuenta con información verificada, el usuario incorpora al cliente al flujo de validación de datos de la plataforma:
   a. Registra la gestión de contacto fallida con el resultado correspondiente.
   b. Activa el proceso de búsqueda de nueva información de contactabilidad a través de la plataforma.
5. Una vez obtenida nueva información, el sistema la somete a validación cruzada. El responsable de datos confirma o descarta la información según el resultado obtenido.
6. El usuario registra el resultado final de la gestión de contactabilidad, indicando si fue exitosa o fallida, junto con las observaciones pertinentes.

---

## 5. Roles y Responsabilidades

| Actor | Responsabilidad principal |
|---|---|
| Subgerente del Área solicitante | Autorizar y co-aprobar las solicitudes de acceso, modificación y revocación del personal a su cargo. |
| Subgerente de Gestión de Datos | Co-aprobar las solicitudes de acceso y velar por la pertinencia del nivel otorgado. |
| Área de Seguridad Bancaria | Ejecutar la asignación y revocación de grupos de seguridad en el Active Directory, conforme al proceso normado. Mantener el registro de asignaciones. |
| Administrador de la plataforma DSH | Validar técnicamente las solicitudes de acceso. Monitorear el desempeño y disponibilidad de la plataforma. Evaluar la efectividad de la contactabilidad e impulsar mejoras continuas en la plataforma para optimizar los resultados. |
| Responsable de Gestión de Datos | Carga a demanda de datos de proveedores externos. Validación y reparación diaria de datos de clientes confirmados. |
| Supervisor (`DSH_SUPERVISOR`) | Supervisión operativa del equipo y escalamiento de incidencias. |
| Agente operativo (`DSH_AGENTE`) | Consulta y registro de gestiones de contactabilidad sobre clientes. |
| Usuario de consulta (`DSH_CONSULTA`) | Acceso de solo lectura para seguimiento y análisis. |

---

## 6. Control de Cambios

| Versión | Fecha | Autor | Descripción |
|---|---|---|---|
| 1.0 | 27/05/2026 | Administración DSH | Versión inicial del documento. |
| 1.1 | 27/05/2026 | Administración DSH | Correcciones: nombre oficial de la plataforma, autenticación via Active Directory, separación de roles entre administrador y Seguridad de la Información, ajuste del procedimiento de acceso por grupos de seguridad, eliminación del procedimiento de reporte diario pendiente de implementación. |
| 1.2 | 27/05/2026 | Administración DSH | Correcciones: uso de "departamento" en lugar de "unidad", separación del procedimiento de datos en carga a demanda de proveedores y gestión diaria de clientes confirmados. Ajuste de nombres: "Área de Seguridad Bancaria" y distinción entre Áreas y Departamento de Gestión de Datos. |
