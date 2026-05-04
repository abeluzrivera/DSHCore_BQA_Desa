Título: Parametrizar catálogos de códigos de error (no hardcodeados) en UI y servidor

Resumen
-------
Los códigos y etiquetas de error para contactos (ej. `ERR_TEL_FMT`, `ERR_MAIL_DOM`, `ERR_DIR_INC`, etc.) están actualmente embebidos en el código fuente tanto en la UI Razor (`Pages/Clientes.cshtml`) como en el backend (`Pages/Dashboard.cshtml.cs` - `ErrorLabels` dictionary). Esto dificulta mantenimiento, localización y extensión de los catálogos, y puede provocar desincronización entre front y back.

Contexto / Evidencia
--------------------
- Fragmento seleccionado en `Pages/Clientes.cshtml` contiene botones con `data-error-code` hardcodeados:
  - `ERR_TEL_FMT`, `ERR_TEL_DIGIT`, `ERR_TEL_NUM`, `ERR_TEL_CODE`, `ERR_TEL_DUMMY`.
- En el backend hay un diccionario estático `ErrorLabels` en `Pages/Dashboard.cshtml.cs` con las mismas claves y sus descripciones.

Impacto
-------
- Añadir/modificar/eliminar códigos requiere cambios en código y nuevos despliegues.
- Riesgo de inconsistencias entre UI y backend (etiquetas, disponibilidad por tipo de contacto).
- Dificulta traducción/parametrización y control por producto/operaciones.

Comportamiento esperado
-----------------------
- Los catálogos de códigos de error deben estar parametrizados (BD o archivo de configuración) y ser consumidos por backend y frontend.
- UI debe renderizar dinámicamente las opciones disponibles por tipo de contacto.
- Backend debe leer el catálogo centralizado para validar/mostrar etiquetas y reglas.

Propuesta de solución (tareas sugeridas)
----------------------------------------
1. Crear tabla de catálogo en BD: `Tbl_Catalogo_Codigos_Error` con columnas: `Codigo (PK)`, `Etiqueta`, `TipoContacto` (opcional), `Activo`, `Orden`, `UsuarioCreacion`, `FechaCreacion`, `UsuarioModificacion`, `FechaModificacion`.
2. Migración + seed inicial con los códigos actuales para evitar ruptura.
3. Implementar repositorio/servicio que cargue y cachee el catálogo en la API (ej. `ICatalogoService.GetErrorCodesByContactType(string tipo)`).
4. Reemplazar el diccionario `ErrorLabels` en `Pages/Dashboard.cshtml.cs` por la inyección del servicio y usar las etiquetas desde BD.
5. Crear endpoint JSON para que la UI pueda solicitar los códigos activos por tipo de contacto.
6. Modificar `Pages/Clientes.cshtml` para renderizar las `cp-reject-pill` dentro de un `foreach` usando los datos recibidos en el modelo o vía fetch desde el endpoint.
7. Añadir pruebas básicas y documentación; agregar una pequeña UI de administración para gestionar el catálogo (opcional pero recomendado).

Archivos afectados (ejemplos)
----------------------------
- `contactabilidad_inteligente.mcv/Pages/Clientes.cshtml`
- `contactabilidad_inteligente.mcv/Pages/Dashboard.cshtml.cs`
- `SDH.Domain/Entities` (si hay enums o constantes relacionadas)
- Repositorio/Infrastructure: crear `Catalogo` entity, repository y migrations

Prioridad: Medium-High
Labels sugeridos: `bug`, `enhancement`, `database`, `refactor`

Notas adicionales
----------------
- Asegurarse de mantener retrocompatibilidad: seedear los valores existentes y mantenerlos activos mientras se realiza el cambio gradual.
- Considerar permisos para la administración del catálogo (solo equipo de operaciones o admins).
- Este cambio ayudará además a centralizar otros catálogos similares en el futuro (etiquetas, tipos de contacto, motivos, etc.).

Asignar a: TBD

