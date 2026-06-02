using SDH.Domain.Enums;

namespace SDH.Domain.Entities.Parametro
{
    /// <summary>
    /// Entidad Hija: Representa un ítem individual dentro de un catálogo.
    /// Su ciclo de vida depende totalmente de GrupoCatalogo.
    /// </summary>
    public class CatalogItem
    {
        public int Id { get; private set; }

        public int GroupId { get; private set; }

        public string ValueCode { get; private set; } = string.Empty;
        public string DisplayName { get; private set; } = string.Empty;
        public int DisplayOrder { get; private set; }
        public bool IsActive { get; private set; }

        public DateTime CreatedAt { get; private set; }
        public string? CreatedBy { get; private set; }
        public DateTime? LastModifiedAt { get; private set; }
        public string? LastModifiedBy { get; private set; }

        // Constructor privado para EF Core
        private CatalogItem() { }

        // Factory method (Se elimina el parámetro idGrupoCatalogo)
        public static CatalogItem Create(
            int id,
            string codigoValor,
            string textoVisual,
            int ordenVisual = 0,
            string? usuarioCreacion = GlobalVariables.SystemUser)
        {
            return new CatalogItem
            {
                Id = id,
                ValueCode = codigoValor,
                DisplayName = textoVisual,
                DisplayOrder = ordenVisual,
                IsActive = true,
                CreatedAt = DateTime.Now,
                CreatedBy = usuarioCreacion
            };
        }

        public void Update(string textoVisual, int ordenVisual, string? usuarioModificacion = null)
        {
            DisplayName = textoVisual;
            DisplayOrder = ordenVisual;
            LastModifiedAt = DateTime.Now;
            LastModifiedBy = usuarioModificacion;
        }

        public void ChangeStatus(bool activo, string? usuarioModificacion = null)
        {
            IsActive = activo;
            LastModifiedAt = DateTime.Now;
            LastModifiedBy = usuarioModificacion;
        }

    }
}