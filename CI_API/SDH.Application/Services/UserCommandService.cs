using Microsoft.Extensions.Logging;
using SDH.Application.Common;
using SDH.Application.DTOs.Users;
using SDH.Application.Ports.Services;
using SDH.Domain.Entities.Seguridad;
using SDH.Domain.Enums;
using SDH.Domain.Ports;
using SDH.Domain.Repositories;
using System.Security.Cryptography;

namespace SDH.Application.Services
{
    /// <summary>
    /// CQRS Write Service para Usuario.
    /// Orquesta la creación, actualización y cambio de estado de usuarios.
    /// Toda lógica de negocio se delega a la entidad de dominio Usuario.
    /// </summary>
    public class UserCommandService(
        IUnitOfWork unitOfWork,
        IUserRepository IUserQueryService,
        IUserQueryService usuarioQueryService,
        CatalogoService catalogoService,
        IPasswordHasher passwordHasher,
        ICurrentUserService currentUserService,
        ILogger<UserCommandService> logger)
    {
        private string CurrentUser =>
            currentUserService.ObtenerUsuarioActual() ?? GlobalVariables.SystemUser;

        // ────────────────────────────────────────────────────────────
        // COMANDOS
        // ────────────────────────────────────────────────────────────

        /// <summary>
        /// Crea un nuevo usuario validando unicidad de código y email.
        /// Retorna el ID del usuario creado.
        /// </summary>
        public async Task<Result<int>> CreateAsync(
            CreateUsuarioCommand command,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!await catalogoService.ValidarCodigoExisteAsync(CatalogGroups.SystemRole, command.RoleCode, cancellationToken))
                    return Result<int>.Failure($"El rol '{command.RoleCode}' no es válido.");

                if (await usuarioQueryService.ExistsByUsernameAsync(command.Username, cancellationToken: cancellationToken))
                    return Result<int>.Failure($"El código de usuario '{command.Username}' ya existe.");

                if (await usuarioQueryService.ExistsByEmailAsync(command.Email, cancellationToken: cancellationToken))
                    return Result<int>.Failure($"El email '{command.Email}' ya está registrado.");

                string rolTextoVisual = await catalogoService.ObtenerTextoVisualAsync(
                    CatalogGroups.SystemRole, command.RoleCode, command.RoleCode, cancellationToken);

                string claveHash = passwordHasher.GenerarHash(command.PlainPassword);

                var usuario = Users.Create(
                    command.Username,
                    command.Email,
                    claveHash,
                    command.FullName,
                    rolTextoVisual,
                    command.CreatedBy);

                if (!command.IsActive)
                    usuario.ChangeStatus(false);

                await IUserQueryService.AddUserAsync(usuario, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);

                logger.LogInformation("Usuario creado: {Codigo} - {Email} - Rol: {Rol}",
                    command.Username, command.Email, rolTextoVisual);

                return Result<int>.Success(usuario.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al crear usuario {Codigo}", command.Username);
                return Result<int>.Failure("Error al crear el usuario.");
            }
        }

        /// <summary>
        /// Actualiza los datos de un usuario existente.
        /// Valida unicidad de código y email excluyendo al propio usuario.
        /// </summary>
        public async Task<Result> UpdateAsync(
            UpdateUsuarioCommand command,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var usuario = await IUserQueryService.GetByIdAsync(command.Id, cancellationToken);
                if (usuario is null)
                    return Result.Failure("Usuario no encontrado.");

                if (!await catalogoService.ValidarCodigoExisteAsync(CatalogGroups.SystemRole, command.RoleCode, cancellationToken))
                    return Result.Failure($"El rol '{command.RoleCode}' no es válido.");

                if (await usuarioQueryService.ExistsByUsernameAsync(command.Username, command.Id, cancellationToken))
                    return Result.Failure($"El código '{command.Username}' ya está en uso.");

                if (await usuarioQueryService.ExistsByEmailAsync(command.Email, command.Id, cancellationToken))
                    return Result.Failure($"El email '{command.Email}' ya está registrado.");

                string rolTextoVisual = await catalogoService.ObtenerTextoVisualAsync(
                    CatalogGroups.SystemRole, command.RoleCode, command.RoleCode, cancellationToken);

                usuario.UpdateProfile(
                    command.FullName,
                    command.Username,
                    command.Email,
                    rolTextoVisual,
                    command.ModifiedBy);

                usuario.ChangeStatus(command.IsActive, command.ModifiedBy);

                await unitOfWork.SaveChangesAsync(cancellationToken);

                logger.LogInformation("Usuario {Id} actualizado por {Modificador}", command.Id, command.ModifiedBy);

                return Result.Success();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al actualizar usuario {Id}", command.Id);
                return Result.Failure("Error al actualizar el usuario.");
            }
        }

        /// <summary>
        /// Invierte el estado activo/inactivo de un usuario.
        /// Retorna el nuevo estado junto con los datos visuales para actualizar la UI.
        /// </summary>
        public async Task<Result<UserStatusToggleDto>> ToggleStatusAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var usuario = await IUserQueryService.GetByIdAsync(id, cancellationToken);
                if (usuario is null)
                    return Result<UserStatusToggleDto>.Failure("Usuario no encontrado.");

                usuario.ChangeStatus(!usuario.IsActive, CurrentUser);
                await unitOfWork.SaveChangesAsync(cancellationToken);

                logger.LogInformation("Estado de usuario {Id} cambiado a {Estado} por {Modificador}",
                    id, usuario.IsActive, CurrentUser);

                return Result<UserStatusToggleDto>.Success(new UserStatusToggleDto(
                    NewStatus: usuario.IsActive,
                    StatusText: usuario.IsActive ? "Activo" : "Inactivo",
                    StatusClass: usuario.IsActive ? "status-active" : "status-inactive"));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al cambiar estado del usuario {Id}", id);
                return Result<UserStatusToggleDto>.Failure("Error al cambiar el estado del usuario.");
            }
        }

        // ────────────────────────────────────────────────────────────
        // UTILIDADES
        // ────────────────────────────────────────────────────────────

        /// <summary>
        /// Genera una contraseña segura aleatoria de 12 caracteres.
        /// Garantiza al menos 1 mayúscula, 1 minúscula, 1 dígito y 1 especial.
        /// </summary>
        public static string GenerateSecurePassword()
        {
            const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lower = "abcdefghijklmnopqrstuvwxyz";
            const string digits = "0123456789";
            const string special = "!@#$%^&*()-_=+[]{}|;:,.<>?";
            const string all = upper + lower + digits + special;

            // Generamos los 4 caracteres obligatorios usando GetInt32(max)
            var password = new List<char>
                {
                    upper[RandomNumberGenerator.GetInt32(upper.Length)],
                    lower[RandomNumberGenerator.GetInt32(lower.Length)],
                    digits[RandomNumberGenerator.GetInt32(digits.Length)],
                    special[RandomNumberGenerator.GetInt32(special.Length)]
                };

            // Completamos los 8 caracteres restantes
            for (int i = 0; i < 8; i++)
            {
                password.Add(all[RandomNumberGenerator.GetInt32(all.Length)]);
            }

            // Mezclamos la lista de forma segura y convertimos a string
            // Usamos OrderBy con un valor aleatorio para el shuffle
            return new string([.. password.OrderBy(_ => RandomNumberGenerator.GetInt32(int.MaxValue))]);
        }
    }
}
