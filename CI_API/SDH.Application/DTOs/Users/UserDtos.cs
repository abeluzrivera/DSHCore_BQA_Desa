namespace SDH.Application.DTOs.Users
{
    // ──────────────────────────────────────────────────
    // DTOs de Lectura (Read) - Usamos records posicionales
    // ──────────────────────────────────────────────────

    /// <summary>
    /// DTO ligero para la fila de la tabla de usuarios.
    /// Proyectado directamente desde BD sin tracking.
    /// </summary>
    public record UserListItemDto(
        int Id,
        string FullName,
        string Email,
        string Username,
        string SystemRole,
        bool IsActive,
        bool IsLockedOut,
        DateTime? LastLoginAt);

    /// <summary>
    /// DTO para el modal de edición. Expone RolSistema como texto visual
    /// (igual a como se almacena en BD). El Controller resuelve el código
    /// del catálogo para el dropdown usando CatalogoService.
    /// </summary>
    public record UserDetailDto(
        int Id,
        string FullName,
        string Username,
        string Email,
        string SystemRole,
        bool IsActive);

    /// <summary>
    /// Resultado del toggle de estado de un usuario.
    /// Contiene los datos necesarios para actualizar la UI sin recargar la tabla.
    /// </summary>
    public record UserStatusToggleDto(
        bool NewStatus,
        string StatusText,
        string StatusClass);

    // ──────────────────────────────────────────────────
    // Commands (Write Side) - Inmutables por diseño
    // ──────────────────────────────────────────────────

    /// <summary>
    /// Command para crear un nuevo usuario.
    /// RolCodigo = código de catálogo (ej: "ADMIN"). El service resuelve el texto visual.
    /// </summary>
    public record CreateUsuarioCommand(
        string FullName,
        string Username,
        string Email,
        string RoleCode,
        string PlainPassword,
        bool IsActive,
        string CreatedBy);

    /// <summary>
    /// Command para actualizar datos de un usuario existente.
    /// No incluye contraseña — operación separada por seguridad.
    /// </summary>
    public record UpdateUsuarioCommand(
        int Id,
        string FullName,
        string Username,
        string Email,
        string RoleCode,
        bool IsActive,
        string ModifiedBy);
}
