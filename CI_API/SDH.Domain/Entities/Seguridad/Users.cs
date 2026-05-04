using SDH.Domain.Enums;
using SDH.Domain.Ports;

namespace SDH.Domain.Entities.Seguridad
{
    /// <summary>
    /// Aggregate Root: Representa un usuario del sistema con sus credenciales y perfil.
    /// </summary>
    public class Users
    {
        public int Id { get; private set; } // Renombrado a Id por convención
        public string Username { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string PasswordHash { get; private set; } = string.Empty;
        public string FullName { get; private set; } = string.Empty;
        public string SystemRole { get; private set; } = string.Empty;

        public DateTime? LastLoginAt { get; private set; }
        public bool IsActive { get; private set; }
        public bool IsDeleted { get; private set; }
        public bool? IsLockedOut { get; private set; } // Modificado a private set para proteger el estado

        // Auditoría
        public DateTime CreatedAt { get; private set; }
        public string? CreatedBy { get; private set; }
        public DateTime? LastModifiedAt { get; private set; }
        public string? LastModifiedBy { get; private set; }

        // Constructor privado para EF Core
        private Users() { }

        /// <summary>
        /// Factory method para crear un nuevo usuario.
        /// </summary>
        public static Users Create(
            string codigoUsuario,
            string email,
            string claveHash,
            string nombreAsesor,
            string rolSistema,
            string? usuarioCreacion = null)
        {
            return new Users
            {
                Username = codigoUsuario,
                Email = email,
                PasswordHash = claveHash,
                FullName = nombreAsesor,
                SystemRole = rolSistema,
                IsActive = true,
                IsLockedOut = false, // Inicializamos explícitamente
                IsDeleted = false,
                CreatedAt = DateTime.Now, // Usamos Now
                CreatedBy = usuarioCreacion ?? GlobalVariables.SystemUser
            };
        }

        // ──────────────────────────────────────────────────
        // COMPORTAMIENTOS DEL NEGOCIO (Métodos)
        // ──────────────────────────────────────────────────

        public void UpdateLastLogin()
        {
            LastLoginAt = DateTime.Now;
        }

        public void ChangeStatus(bool activo, string? usuarioModificacion = null)
        {
            IsActive = activo;
        }

        public void ChangeLockoutStatus(bool bloqueado, string? usuarioModificacion = null)
        {
            IsLockedOut = bloqueado;
        }

        public void UpdatePassword(string nuevaClaveHash, string? usuarioModificacion = null)
        {
            PasswordHash = nuevaClaveHash;
        }

        public void UpdateProfile(string nombreAsesor, string codigoUsuario, string email, string rolSistema, string? usuarioModificacion = null)
        {
            FullName = nombreAsesor;
            Username = codigoUsuario;
            Email = email;
            SystemRole = rolSistema;
        }

        public void MarkAsDeleted(string? usuarioModificacion)
        {
            IsDeleted = true;
            IsActive = false;
        }

        // ──────────────────────────────────────────────────
        // MÉTODOS AUXILIARES PRIVADOS
        // ──────────────────────────────────────────────────


        public void Authenticate(string clavePlana, IPasswordHasher hasher)
        {
            if (!IsActive)
                throw new InvalidOperationException("El usuario está inactivo.");

            if (IsLockedOut == true)
                throw new InvalidOperationException("El usuario está bloqueado.");

            if (!hasher.Verificar(clavePlana, PasswordHash))
                throw new UnauthorizedAccessException("Credenciales incorrectas.");

            // Si pasó todas las validaciones, actualizamos el acceso
            UpdateLastLogin();
        }
    }
}