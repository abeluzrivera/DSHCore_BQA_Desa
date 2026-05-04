using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SDH.Domain.Entities.Parametro;
using SDH.Domain.Enums;
using SDH.Domain.Extensions;
using SDH.infrastructure.Persistence.Data;

namespace SDH.infrastructure.Persistence.Seeders
{
    /// <summary>
    /// Clase para inicializar y sincronizar datos de catálogos del sistema.
    /// Orientada al Dominio: Utiliza el Aggregate Root (GrupoCatalogo) para gestionar sus ítems.
    /// </summary>
    public static class CatalogoSeeder
    {

        /// <summary>
        /// Sincroniza grupos de catálogos e items utilizando las reglas del Dominio.
        /// </summary>
        public static async Task SeedAsync(ApplicationDbContext context, Microsoft.Extensions.Logging.ILogger logger)
        {
            try
            {
                logger.LogInformation("Iniciando sincronización de catálogos orientada al Dominio...");

                // 1. Obtener las definiciones maestras
                var gruposDefinidos = ObtenerGruposDefinidos();
                var itemsDefinidos = ObtenerItemsDefinidos();

                // 2. Cargar Agregados existentes (Padre + Hijos) desde la BD
                // Cargamos con .Include para que el Root pueda gestionar sus ítems en memoria
                var gruposExistentes = await context.CatalogGroup
                    .Include(g => g.Items)
                    .ToListAsync();

                int gruposNuevos = 0;
                int itemsSincronizados = 0;

                // 3. Procesar cada Grupo definido
                foreach (var defGrupo in gruposDefinidos)
                {
                    var grupo = gruposExistentes.FirstOrDefault(g => g.GroupName == defGrupo.NombreGrupo);

                    // Si el grupo no existe, lo creamos usando el Factory del Dominio
                    if (grupo == null)
                    {
                        logger.LogDebug("Creando nuevo Aggregate Root: {NombreGrupo}", defGrupo.NombreGrupo);
                        grupo = CatalogGroup.Create(defGrupo.NombreGrupo, defGrupo.Descripcion, defGrupo.EsSistema, GlobalVariables.SystemUser);
                        context.CatalogGroup.Add(grupo);
                        gruposNuevos++;
                    }

                    // 4. Sincronizar los ítems de este grupo específico
                    var itemsParaEsteGrupo = itemsDefinidos
                        .Where(i => i.NombreGrupo == defGrupo.NombreGrupo)
                        .ToList();

                    itemsSincronizados += SincronizarItemsDelAgregado(grupo, itemsParaEsteGrupo, logger);
                }

                // 5. Persistir todos los cambios de una sola vez
                if (context.ChangeTracker.HasChanges())
                {
                    await context.SaveChangesAsync();
                    logger.LogInformation("Sincronización completada. Grupos nuevos: {G}, Ítems procesados: {I}", gruposNuevos, itemsSincronizados);
                }
                else
                {
                    logger.LogInformation("No se detectaron cambios. Los catálogos ya están sincronizados.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error crítico durante el seeding de catálogos");
                throw;
            }
        }



        /// <summary>
        /// Sincroniza la colección de ítems dentro de un GrupoCatalogo (Aggregate Root).
        /// </summary>
        private static int SincronizarItemsDelAgregado(
            CatalogGroup grupo,
            List<(int Id, string NombreGrupo, string CodigoValor, string TextoVisual, int OrdenVisual, bool EstaActivo)> defs,
            Microsoft.Extensions.Logging.ILogger logger)
        {
            int procesados = 0;
            var codigosEnDefinicion = defs.Select(d => d.CodigoValor).ToHashSet();
            foreach (var (def, itemExistente) in
            // A. Agregar o ActualizarNombres ítems según la definición
            from def in defs
            let itemExistente = grupo.Items.FirstOrDefault(i => i.ValueCode == def.CodigoValor)
            select (def, itemExistente))
            {
                if (itemExistente == null)
                {
                    // El Root agrega un nuevo hijo usando el comportamiento del Dominio
                    var nuevoItem = CatalogItem.Create(def.Id, def.CodigoValor, def.TextoVisual, def.OrdenVisual, GlobalVariables.SystemUser);
                    grupo.AddItem(nuevoItem);
                    logger.LogDebug("Ítem nuevo agregado a {Grupo}: {Codigo}", grupo.GroupName, def.CodigoValor);
                }
                else
                {
                    // ActualizarNombres datos si han cambiado
                    if (itemExistente.DisplayName != def.TextoVisual || itemExistente.DisplayOrder != def.OrdenVisual)
                    {
                        itemExistente.Update(def.TextoVisual, def.OrdenVisual, GlobalVariables.SystemUser);
                    }

                    // Sincronizar estado Activo/Inactivo
                    if (itemExistente.IsActive != def.EstaActivo)
                    {
                        itemExistente.ChangeStatus(def.EstaActivo, GlobalVariables.SystemUser);
                    }
                }
                procesados++;
            }

            // B. Borrado Lógico (Inactivar ítems que ya no están en la lista definida)
            var itemsParaDesactivar = grupo.Items
                .Where(i => i.IsActive && !codigosEnDefinicion.Contains(i.ValueCode))
                .ToList();

            foreach (var obsoleto in itemsParaDesactivar)
            {
                logger.LogWarning("Inactivando ítem obsoleto en {Grupo}: {Codigo}", grupo.GroupName, obsoleto.ValueCode);
                obsoleto.ChangeStatus(false, GlobalVariables.SystemUser);
            }

            return procesados;
        }

        private static List<(string NombreGrupo, string Descripcion, bool EsSistema)> ObtenerGruposDefinidos()
        {
            return
            [
                (CatalogGroups.UserStatus, "Estados posibles de una cuenta de usuario", true),
                (CatalogGroups.FileStatus, "Ciclo de vida del procesamiento de archivos masivos", true),
                (CatalogGroups.ContactType, "Tipos de medios de contacto del cliente", true),
                (CatalogGroups.SystemRole, "Roles de acceso al aplicativo", true),
                (CatalogGroups.AccountingType, "Clasificación contable para análisis financiero", true),
                (CatalogGroups.ContactabilityStatus, "Estado final de la validación del dato de contacto", true),
                (CatalogGroups.ContactabilityStatus, "Detalle del por qué falló la validación del dato", true),
                (CatalogGroups.CustomerStatus, "Estado general de validación del cliente", true),
                (CatalogGroups.IdentificationType, "Tipos de documentos de identidad legales", true)
            ];
        }

        private static List<(int Id, string NombreGrupo, string CodigoValor, string TextoVisual, int OrdenVisual, bool EstaActivo)> ObtenerItemsDefinidos()
        {
            return
            [
                // ==========================================
                // ESTADO_USUARIO
                // ==========================================
                ((int)EnumUserStatus.Active, CatalogGroups.UserStatus, EnumUserStatus.Active.GetValueCatalog(), "Activo", 1, true),
                ((int)EnumUserStatus.Inactive, CatalogGroups.UserStatus, EnumUserStatus.Inactive.GetValueCatalog(), "Inactivo", 2, true),
                ((int)EnumUserStatus.Locked, CatalogGroups.UserStatus, EnumUserStatus.Locked.GetValueCatalog(), "Bloqueado por Intentos", 3, true),
                ((int)EnumUserStatus.PasswordChangeRequired, CatalogGroups.UserStatus, EnumUserStatus.PasswordChangeRequired.GetValueCatalog(), "Requiere Cambio de Clave", 4, true),
                ((int)EnumUserStatus.Suspended, CatalogGroups.UserStatus, EnumUserStatus.Suspended.GetValueCatalog(), "Suspendido (Investigación)", 5, true),

                // ==========================================
                // ESTADO_ARCHIVO
                // ==========================================
                ((int)EnumFileStatus.Pending, CatalogGroups.FileStatus, EnumFileStatus.Pending.GetValueCatalog(), "Pendiente en Cola", 1, true),
                ((int)EnumFileStatus.Validating, CatalogGroups.FileStatus, EnumFileStatus.Validating.GetValueCatalog(), "Validando Estructura", 2, true),
                ((int)EnumFileStatus.Processing, CatalogGroups.FileStatus, EnumFileStatus.Processing.GetValueCatalog(), "Procesando Datos", 3, true),
                ((int)EnumFileStatus.Completed, CatalogGroups.FileStatus, EnumFileStatus.Completed.GetValueCatalog(), "Procesado Exitosamente", 4, true),
                ((int)EnumFileStatus.Partial, CatalogGroups.FileStatus, EnumFileStatus.Partial.GetValueCatalog(), "Procesado Parcialmente", 5, true),
                ((int)EnumFileStatus.Failed, CatalogGroups.FileStatus, EnumFileStatus.Failed.GetValueCatalog(), "Error Crítico", 6, true),
                ((int)EnumFileStatus.Cancelled, CatalogGroups.FileStatus, EnumFileStatus.Cancelled.GetValueCatalog(), "Cancelado por el Usuario", 7, true),

                // ==========================================
                // ROL_SISTEMA
                // ==========================================
                ((int)EnumSystemRole.SuperAdmin, CatalogGroups.SystemRole, EnumSystemRole.SuperAdmin.GetValueCatalog(), "Super Administrador", 1, true),
                ((int)EnumSystemRole.Administrator, CatalogGroups.SystemRole, EnumSystemRole.Administrator.GetValueCatalog(), "Administrador de Agencia", 2, true),
                ((int)EnumSystemRole.Auditor, CatalogGroups.SystemRole, EnumSystemRole.Auditor.GetValueCatalog(), "Auditoría y Cumplimiento", 4, true),
                ((int)EnumSystemRole.Operator, CatalogGroups.SystemRole, EnumSystemRole.Operator.GetValueCatalog(), "Usuario Operativo", 5, true),

                // ==========================================
                // TIPO_CONTACTO
                // ==========================================
                ((int)EnumContactabilityType.Phone, CatalogGroups.ContactType, EnumContactabilityType.Phone.GetValueCatalog(), "Teléfono Celular", 1, true),
                ((int)EnumContactabilityType.Email, CatalogGroups.ContactType, EnumContactabilityType.Email.GetValueCatalog(), "Correo Electrónico", 2, true),
                ((int)EnumContactabilityType.Conventional, CatalogGroups.ContactType, EnumContactabilityType.Conventional.GetValueCatalog(), "Teléfono Convencional", 3, true),
                ((int)EnumContactabilityType.WhatsApp, CatalogGroups.ContactType, EnumContactabilityType.WhatsApp.GetValueCatalog(), "WhatsApp", 4, true),
                ((int)EnumContactabilityType.HomeAddress, CatalogGroups.ContactType, EnumContactabilityType.HomeAddress.GetValueCatalog(), "Dirección Domicilio", 5, true),
                ((int)EnumContactabilityType.WorkAddress, CatalogGroups.ContactType, EnumContactabilityType.WorkAddress.GetValueCatalog(), "Dirección de Trabajo", 6, true),
                ((int)EnumContactabilityType.PersonalReference, CatalogGroups.ContactType, EnumContactabilityType.PersonalReference.GetValueCatalog(), "Referencia Personal", 7, true),
                ((int)EnumContactabilityType.CommercialReference, CatalogGroups.ContactType, EnumContactabilityType.CommercialReference.GetValueCatalog(), "Referencia Comercial", 8, true),
                ((int)EnumContactabilityType.Emergency, CatalogGroups.ContactType, EnumContactabilityType.Emergency.GetValueCatalog(), "Contacto de Emergencia", 9, true),
                ((int)EnumContactabilityType.SocialNetwork, CatalogGroups.ContactType, EnumContactabilityType.SocialNetwork.GetValueCatalog(), "Redes Sociales (LinkedIn/FB)", 10, true),

                // ==========================================
                // ESTADO_CLIENTE
                // ==========================================
                ((int)EnumCustomerStatus.Prospect, CatalogGroups.CustomerStatus, EnumCustomerStatus.Prospect.GetValueCatalog(), "Prospecto / Lead", 1, true),
                ((int)EnumCustomerStatus.Pending, CatalogGroups.CustomerStatus, EnumCustomerStatus.Pending.GetValueCatalog(), "Pendiente de Validación", 2, true),
                ((int)EnumCustomerStatus.Verified, CatalogGroups.CustomerStatus, EnumCustomerStatus.Verified.GetValueCatalog(), "Cliente Activo y Verificado", 3, true),
                ((int)EnumCustomerStatus.Inactive, CatalogGroups.CustomerStatus, EnumCustomerStatus.Inactive.GetValueCatalog(), "Cliente Inactivo / Ex-Cliente", 4, true),
                ((int)EnumCustomerStatus.Rejected, CatalogGroups.CustomerStatus, EnumCustomerStatus.Rejected.GetValueCatalog(), "Rechazado (No Califica)", 5, true),
                ((int)EnumCustomerStatus.Blocked, CatalogGroups.CustomerStatus, EnumCustomerStatus.Blocked.GetValueCatalog(), "Bloqueado / Lista Negra", 6, true),

                // ==========================================
                // ESTADO_CONTACTABILIDAD
                // ==========================================
                ((int)EnumContactabilityStatus.Verified, CatalogGroups.ContactabilityStatus, EnumContactabilityStatus.Verified.GetValueCatalog(), "Dato Verificado (Exitoso)", 1, true),
                ((int)EnumContactabilityStatus.Pending, CatalogGroups.ContactabilityStatus, EnumContactabilityStatus.Pending.GetValueCatalog(), "Pendiente de Validar", 2, true),
                ((int)EnumContactabilityStatus.NoResponse, CatalogGroups.ContactabilityStatus, EnumContactabilityStatus.NoResponse.GetValueCatalog(), "No Responde / Inalcanzable", 3, true),
                ((int)EnumContactabilityStatus.Expired, CatalogGroups.ContactabilityStatus, EnumContactabilityStatus.Expired.GetValueCatalog(), "Dato Caducado / Desactualizado", 4, true),
                ((int)EnumContactabilityStatus.Error, CatalogGroups.ContactabilityStatus, EnumContactabilityStatus.Error.GetValueCatalog(), "Error Permanente (Inválido)", 5, true),
                ((int)EnumContactabilityStatus.PendingLOPDP, CatalogGroups.ContactabilityStatus, EnumContactabilityStatus.PendingLOPDP.GetValueCatalog(), "Pendiente de Aprobacion de LOPDP", 6, true),
                ((int)EnumContactabilityStatus.AprovalLOPDP, CatalogGroups.ContactabilityStatus, EnumContactabilityStatus.AprovalLOPDP.GetValueCatalog(), "Aprobado de LOPDP", 7, true),
                ((int)EnumContactabilityStatus.DenyLOPDP, CatalogGroups.ContactabilityStatus, EnumContactabilityStatus.DenyLOPDP.GetValueCatalog(), "Denegado de LOPDP", 8, true),

                // ==========================================
                // MOTIVO_ERROR_CONTACTO
                // ==========================================
                ((int)EnumContactabilityStatus.EmailFormato, CatalogGroups.ContactabilityStatus, EnumContactabilityStatus.EmailFormato.GetValueCatalog(), "Formato de email inválido (falta @ o dominio)", 1, true),
                ((int)EnumContactabilityStatus.EmailDominio, CatalogGroups.ContactabilityStatus, EnumContactabilityStatus.EmailDominio.GetValueCatalog(), "Dominio de correo inexistente o inalcanzable", 2, true),
                ((int)EnumContactabilityStatus.EmailTemporal, CatalogGroups.ContactabilityStatus, EnumContactabilityStatus.EmailTemporal.GetValueCatalog(), "Uso de correo temporal o desechable no permitido", 3, true),
                ((int)EnumContactabilityStatus.EmailDuplicado, CatalogGroups.ContactabilityStatus, EnumContactabilityStatus.EmailDuplicado.GetValueCatalog(), "Email ya registrado en otro cliente", 4, true),
                ((int)EnumContactabilityStatus.EmailRebote, CatalogGroups.ContactabilityStatus, EnumContactabilityStatus.EmailRebote.GetValueCatalog(), "El correo reporta rebote previo (Bounced)", 5, true),
                ((int)EnumContactabilityStatus.EmailCaracteres, CatalogGroups.ContactabilityStatus, EnumContactabilityStatus.EmailCaracteres.GetValueCatalog(), "El correo contiene caracteres inválidos o espacios", 6, true),

                ((int)EnumContactabilityStatus.TelFormato, CatalogGroups.ContactabilityStatus, EnumContactabilityStatus.TelFormato.GetValueCatalog(), "Formato de teléfono inválido", 7, true),
                ((int)EnumContactabilityStatus.TelDigitos, CatalogGroups.ContactabilityStatus, EnumContactabilityStatus.TelDigitos.GetValueCatalog(), "Cantidad de dígitos incorrecta para el país", 8, true),
                ((int)EnumContactabilityStatus.TelNoNumerico, CatalogGroups.ContactabilityStatus, EnumContactabilityStatus.TelNoNumerico.GetValueCatalog(), "El valor contiene letras o no es numérico", 9, true),
                ((int)EnumContactabilityStatus.TelCodigo, CatalogGroups.ContactabilityStatus, EnumContactabilityStatus.TelCodigo.GetValueCatalog(), "Código de área o prefijo no soportado", 10, true),
                ((int)EnumContactabilityStatus.TelFicticio, CatalogGroups.ContactabilityStatus, EnumContactabilityStatus.TelFicticio.GetValueCatalog(), "Número ficticio o repetido (Ej. 099999999)", 11, true),
                ((int)EnumContactabilityStatus.TelDesconectado, CatalogGroups.ContactabilityStatus, EnumContactabilityStatus.TelDesconectado.GetValueCatalog(), "Número reportado como desconectado o inactivo", 12, true),
                ((int)EnumContactabilityStatus.TelDuplicado, CatalogGroups.ContactabilityStatus, EnumContactabilityStatus.TelDuplicado.GetValueCatalog(), "Número de teléfono ya asociado a otro cliente", 13, true),

                ((int)EnumContactabilityStatus.DirCorta, CatalogGroups.ContactabilityStatus, EnumContactabilityStatus.DirCorta.GetValueCatalog(), "Dirección demasiado corta (falta detalle)", 14, true),
                ((int)EnumContactabilityStatus.DirLarga, CatalogGroups.ContactabilityStatus, EnumContactabilityStatus.DirLarga.GetValueCatalog(), "Dirección excede la longitud máxima permitida", 15, true),
                ((int)EnumContactabilityStatus.DirIncompleta, CatalogGroups.ContactabilityStatus, EnumContactabilityStatus.DirIncompleta.GetValueCatalog(), "Dirección incompleta (falta número o intersección)", 16, true),
                ((int)EnumContactabilityStatus.DirCasillero, CatalogGroups.ContactabilityStatus, EnumContactabilityStatus.DirCasillero.GetValueCatalog(), "No se permiten casilleros postales (P.O. Box)", 17, true),
                ((int)EnumContactabilityStatus.DirGenerica, CatalogGroups.ContactabilityStatus, EnumContactabilityStatus.DirGenerica.GetValueCatalog(), "Dirección genérica inválida (Ej. \"S/N\", \"Conocida\")", 18, true),
                ((int)EnumContactabilityStatus.DirCaracteres, CatalogGroups.ContactabilityStatus, EnumContactabilityStatus.DirCaracteres.GetValueCatalog(), "La dirección contiene caracteres no permitidos", 19, true),
                ((int)EnumContactabilityStatus.DirGeoInconsistente, CatalogGroups.ContactabilityStatus, EnumContactabilityStatus.DirGeoInconsistente.GetValueCatalog(), "Inconsistencia entre ciudad, provincia y código postal", 20, true),

                // ==========================================
                // TIPO_IDENTIFICACION
                // ==========================================
                ((int)EnumIdentificationType.DNI, CatalogGroups.IdentificationType, EnumIdentificationType.DNI.GetValueCatalog(), "Cédula de Identidad", 1, true),
                ((int)EnumIdentificationType.RUC, CatalogGroups.IdentificationType, EnumIdentificationType.RUC.GetValueCatalog(), "Registro Único de Contribuyentes", 2, true),
                ((int)EnumIdentificationType.PAS, CatalogGroups.IdentificationType, EnumIdentificationType.PAS.GetValueCatalog(), "Pasaporte Extranjero", 3, true),
                ((int)EnumIdentificationType.OTR, CatalogGroups.IdentificationType, EnumIdentificationType.OTR.GetValueCatalog(), "Otro Documento", 4, true)
            ];
        }


    }
}